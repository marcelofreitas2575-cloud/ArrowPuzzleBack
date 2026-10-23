using ArrowPuzzle.Gateway.Application;
using ArrowPuzzle.Gateway.Application.Levels.GetLevel;
using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Gateway.Application.Progress.CompleteLevel;
using ArrowPuzzle.Gateway.Application.Progress.GetProgress;
using ArrowPuzzle.Gateway.Application.Sessions.ApplyMove;
using ArrowPuzzle.Gateway.Application.Sessions.GetSession;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.Gateway.Application.Tests;

public class GatewayHandlersTests
{
    [Fact]
    public async Task GetLevel_DelegatesToBackend()
    {
        var level = new LevelDto("002", new BoardDto(4, 4, Array.Empty<ArrowDto>()));
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(level),
            new FakeGameSessionBackend(),
            new FakePlayerProgressBackend());

        var result = await mediator.Send(new GetLevelQuery("002"));

        Assert.NotNull(result);
        Assert.Equal("002", result!.LevelId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetLevel_EmptyLevelId_Throws(string levelId)
    {
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(null),
            new FakeGameSessionBackend(),
            new FakePlayerProgressBackend());

        await Assert.ThrowsAsync<ArgumentException>(() => mediator.Send(new GetLevelQuery(levelId)));
    }

    [Fact]
    public async Task ApplyMove_DelegatesToBackend()
    {
        var expected = new ApplyMoveResultDto("Moved", new BoardDto(3, 3, Array.Empty<ArrowDto>()), true, false);
        var gameSession = new FakeGameSessionBackend { ApplyMoveResult = expected };
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(null),
            gameSession,
            new FakePlayerProgressBackend());

        var result = await mediator.Send(new ApplyMoveCommand("sess-1", "arrow-1"));

        Assert.Equal("Moved", result.Outcome);
    }

    [Fact]
    public async Task ApplyMove_EmptySessionId_Throws()
    {
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(null),
            new FakeGameSessionBackend(),
            new FakePlayerProgressBackend());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            mediator.Send(new ApplyMoveCommand(" ", "arrow-1")));
    }

    [Fact]
    public async Task GetSession_WhenMissing_ReturnsNull()
    {
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(null),
            new FakeGameSessionBackend(),
            new FakePlayerProgressBackend());

        var result = await mediator.Send(new GetSessionQuery("missing"));

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProgress_DelegatesToBackend()
    {
        var progress = new PlayerProgressDto("user-a", new[] { "001", "002" });
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(null),
            new FakeGameSessionBackend(),
            new FakePlayerProgressBackend(progress));

        var result = await mediator.Send(new GetProgressQuery("user-a"));

        Assert.Equal(2, result.CompletedLevelIds.Count);
    }

    [Fact]
    public async Task CompleteLevel_DelegatesToBackend()
    {
        var progress = new PlayerProgressDto("user-b", new[] { "003" });
        var backend = new FakePlayerProgressBackend(progress);
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(null),
            new FakeGameSessionBackend(),
            backend);

        var result = await mediator.Send(new CompleteLevelCommand("user-b", "003"));

        Assert.Contains("003", result.CompletedLevelIds);
    }

    [Fact]
    public async Task CompleteLevel_EmptyUserId_Throws()
    {
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(null),
            new FakeGameSessionBackend(),
            new FakePlayerProgressBackend());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            mediator.Send(new CompleteLevelCommand("", "001")));
    }

    private static IMediator CreateMediator(
        ILevelCatalogBackend levelCatalog,
        IGameSessionBackend gameSession,
        IPlayerProgressBackend playerProgress)
    {
        var services = new ServiceCollection();
        services.AddGatewayApplication();
        services.AddSingleton(levelCatalog);
        services.AddSingleton(gameSession);
        services.AddSingleton(playerProgress);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private sealed class FakeLevelCatalogBackend(LevelDto? level) : ILevelCatalogBackend
    {
        public Task<LevelDto?> GetLevelAsync(string levelId, CancellationToken cancellationToken = default) =>
            Task.FromResult(level);
    }

    private sealed class FakeGameSessionBackend : IGameSessionBackend
    {
        public ApplyMoveResultDto? ApplyMoveResult { get; init; }

        public SessionDto? Session { get; init; }

        public Task<SessionDto> CreateSessionAsync(BoardDto initialBoard, CancellationToken cancellationToken = default) =>
            Task.FromResult(new SessionDto("generated", initialBoard));

        public Task<ApplyMoveResultDto> ApplyMoveAsync(
            string sessionId,
            string arrowId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ApplyMoveResult ?? new ApplyMoveResultDto("Ok", new BoardDto(1, 1, Array.Empty<ArrowDto>()), true, false));

        public Task<SessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Session);
    }

    private sealed class FakePlayerProgressBackend(PlayerProgressDto? progress = null) : IPlayerProgressBackend
    {
        public Task<PlayerProgressDto> GetProgressAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(progress ?? new PlayerProgressDto(userId, Array.Empty<string>()));

        public Task<PlayerProgressDto> CompleteLevelAsync(
            string userId,
            string levelId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(progress ?? new PlayerProgressDto(userId, new[] { levelId }));
    }
}
