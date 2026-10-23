using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Gateway.Application.Ports;
using ArrowPuzzle.Gateway.Application.Sessions.StartLevel;
using ArrowPuzzle.Gateway.Application;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.Gateway.Application.Tests;

public class StartLevelHandlerTests
{
    [Fact]
    public async Task StartLevel_WhenLevelMissing_ThrowsKeyNotFound()
    {
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(null),
            new FakeGameSessionBackend());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mediator.Send(new StartLevelCommand("999")));
    }

    [Fact]
    public async Task StartLevel_WhenLevelExists_CreatesSession()
    {
        var board = new BoardDto(3, 3, Array.Empty<ArrowDto>());
        var level = new LevelDto("001", board);
        var session = new SessionDto("sess-1", board);
        var mediator = CreateMediator(
            new FakeLevelCatalogBackend(level),
            new FakeGameSessionBackend(session));

        var result = await mediator.Send(new StartLevelCommand("001"));

        Assert.Equal("sess-1", result.SessionId);
    }

    private static IMediator CreateMediator(
        ILevelCatalogBackend levelCatalog,
        IGameSessionBackend gameSession)
    {
        var services = new ServiceCollection();
        services.AddGatewayApplication();
        services.AddSingleton(levelCatalog);
        services.AddSingleton(gameSession);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private sealed class FakeLevelCatalogBackend(LevelDto? level) : ILevelCatalogBackend
    {
        public Task<LevelDto?> GetLevelAsync(string levelId, CancellationToken cancellationToken = default) =>
            Task.FromResult(level);
    }

    private sealed class FakeGameSessionBackend(SessionDto? session = null) : IGameSessionBackend
    {
        public Task<SessionDto> CreateSessionAsync(BoardDto board, CancellationToken cancellationToken = default) =>
            Task.FromResult(session ?? new SessionDto("generated", board));

        public Task<ApplyMoveResultDto> ApplyMoveAsync(
            string sessionId,
            string arrowId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<SessionDto?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
