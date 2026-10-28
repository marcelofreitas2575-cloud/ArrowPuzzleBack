using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ArrowPuzzle.BuildingBlocks.Observability;
using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.LevelGeneration;
using Xunit;

namespace ArrowPuzzle.Gateway.E2e.Tests;

[Trait("Category", "E2e")]
public sealed class GatewayGuestWinFlowTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public async Task Health_returns_correlation_id_header()
    {
        var client = CreateClient();
        var correlationId = "e2e-correlation-" + Guid.NewGuid().ToString("N");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation(CorrelationIdContext.HeaderName, correlationId);

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues(CorrelationIdContext.HeaderName, out var values));
        Assert.Equal(correlationId, values!.Single());
    }

    [Fact]
    public async Task Guest_dev_token_start_session_solve_and_complete_level_001()
    {
        var client = CreateClient();
        var token = await RequestGuestAccessTokenAsync(client);

        using var authed = CreateClient();
        authed.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        const string levelId = "001";
        using var levelResponse = await authed.GetAsync($"/api/v1/levels/{levelId}");
        levelResponse.EnsureSuccessStatusCode();

        using var sessionResponse = await authed.PostAsJsonAsync("/api/v1/sessions", new { levelId });
        sessionResponse.EnsureSuccessStatusCode();
        var sessionJson = await sessionResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var sessionId = sessionJson.GetProperty("sessionId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(sessionId));

        var board = E2eBoardMapper.ToDomainBoard(sessionJson.GetProperty("board"));
        var solve = PuzzleSolver.TrySolve(board);
        Assert.True(solve.IsSolvable);
        Assert.NotNull(solve.RemovalOrder);

        foreach (var arrowId in solve.RemovalOrder)
        {
            using var moveResponse = await authed.PostAsJsonAsync(
                $"/api/v1/sessions/{sessionId}/moves",
                new { arrowId = arrowId.ToString() });
            moveResponse.EnsureSuccessStatusCode();
        }

        using var getSessionResponse = await authed.GetAsync($"/api/v1/sessions/{sessionId}");
        getSessionResponse.EnsureSuccessStatusCode();
        var sessionAfter = await getSessionResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        Assert.True(sessionAfter.GetProperty("isPuzzleComplete").GetBoolean());

        using var completeResponse = await authed.PostAsJsonAsync(
            "/api/v1/me/progress/complete",
            new { levelId, sessionId });
        completeResponse.EnsureSuccessStatusCode();

        using var progressResponse = await authed.GetAsync("/api/v1/me/progress");
        progressResponse.EnsureSuccessStatusCode();
        var progress = await progressResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var completed = progress.GetProperty("completedLevelIds").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains(levelId, completed);
    }

    private static HttpClient CreateClient()
    {
        var baseUrl = Environment.GetEnvironmentVariable("GATEWAY_E2E_URL");
        Assert.False(
            string.IsNullOrWhiteSpace(baseUrl),
            "Set GATEWAY_E2E_URL (e.g. http://127.0.0.1:5100) to run E2E tests.");

        return new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromMinutes(2),
        };
    }

    private static async Task<string> RequestGuestAccessTokenAsync(HttpClient client)
    {
        using var tokenResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/dev-token",
            new { username = "guest-player", password = "guest-player" });
        tokenResponse.EnsureSuccessStatusCode();
        var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var accessToken = tokenJson.GetProperty("access_token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        return accessToken!;
    }
}

internal static class E2eBoardMapper
{
    public static Board ToDomainBoard(JsonElement boardElement)
    {
        var width = boardElement.GetProperty("width").GetInt32();
        var height = boardElement.GetProperty("height").GetInt32();
        var arrows = boardElement.GetProperty("arrows").EnumerateArray().Select(MapArrow).ToList();
        return Board.Create(width, height, arrows);
    }

    private static Arrow MapArrow(JsonElement arrow)
    {
        var id = ArrowId.From(arrow.GetProperty("id").GetString()!);
        var direction = Enum.Parse<Direction>(arrow.GetProperty("direction").GetString()!, ignoreCase: true);

        if (arrow.TryGetProperty("path", out var pathElement) && pathElement.ValueKind == JsonValueKind.Array)
        {
            var cells = pathElement.EnumerateArray()
                .Select(p => new Cell(p.GetProperty("row").GetInt32(), p.GetProperty("column").GetInt32()))
                .ToList();
            if (cells.Count >= 2)
            {
                return Arrow.CreatePolyline(id, direction, cells);
            }
        }

        var headRow = arrow.GetProperty("headRow").GetInt32();
        var headColumn = arrow.GetProperty("headColumn").GetInt32();
        var length = arrow.GetProperty("length").GetInt32();
        return Arrow.Create(id, new Cell(headRow, headColumn), direction, length);
    }
}
