using ArrowPuzzle.GameSession.Application.Sessions.ApplyMove;
using ArrowPuzzle.GameSession.Application.Sessions.CreateSession;
using ArrowPuzzle.GameSession.Application.Sessions.GetSession;
using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.GameSession.Application;
using ArrowPuzzle.GameSession.Application.Ports;
using ArrowPuzzle.GameSession.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.GameSession.Application.Tests;

public class SessionHandlersTests
{
    private static IMediator CreateMediator()
    {
        var services = new ServiceCollection();
        services.AddGameSessionApplication();
        services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task CreateSession_ThenGetSession_ReturnsSameBoard()
    {
        var mediator = CreateMediator();
        var arrow = Arrow.Create(ArrowId.New(), new Cell(0, 0), Direction.Right, length: 1);
        var board = Board.Create(3, 3, new[] { arrow });

        var created = await mediator.Send(new CreateSessionCommand(board));
        var loaded = await mediator.Send(new GetSessionQuery(created.SessionId));

        Assert.NotNull(loaded);
        Assert.Equal(created.SessionId, loaded!.Id);
        Assert.Single(loaded.Board.Arrows);
    }

    [Fact]
    public async Task ApplyMove_UpdatesPersistedSession()
    {
        var mediator = CreateMediator();
        var arrowId = ArrowId.New();
        var arrow = Arrow.Create(arrowId, new Cell(0, 0), Direction.Right, length: 1);
        var board = Board.Create(4, 1, new[] { arrow });
        var created = await mediator.Send(new CreateSessionCommand(board));

        var move = await mediator.Send(new ApplyMoveCommand(created.SessionId, arrowId));

        Assert.Equal(SlideOutcome.ExitedBoard, move.Outcome);
        Assert.True(move.IsPuzzleComplete);

        var loaded = await mediator.Send(new GetSessionQuery(created.SessionId));
        Assert.NotNull(loaded);
        Assert.Empty(loaded!.Board.Arrows);
    }

    [Fact]
    public async Task ApplyMove_MovedAndStopped_DoesNotPersistPartialSlide()
    {
        var mediator = CreateMediator();
        var wallId = ArrowId.New();
        var sliderId = ArrowId.New();
        var wall = Arrow.Create(wallId, new Cell(0, 3), Direction.Down, length: 1);
        var slider = Arrow.Create(sliderId, new Cell(0, 0), Direction.Right, length: 1);
        var board = Board.Create(4, 4, new[] { wall, slider });
        var created = await mediator.Send(new CreateSessionCommand(board));

        var move = await mediator.Send(new ApplyMoveCommand(created.SessionId, sliderId));

        Assert.Equal(SlideOutcome.MovedAndStopped, move.Outcome);
        Assert.Equal(new Cell(0, 0), move.Board.GetArrow(sliderId).Head);

        var loaded = await mediator.Send(new GetSessionQuery(created.SessionId));
        Assert.NotNull(loaded);
        Assert.Equal(new Cell(0, 0), loaded!.Board.GetArrow(sliderId).Head);
    }

    [Fact]
    public async Task GetSession_WhenMissing_ReturnsNull()
    {
        var mediator = CreateMediator();
        var result = await mediator.Send(new GetSessionQuery(SessionId.New()));
        Assert.Null(result);
    }

    [Fact]
    public async Task ApplyMove_WhenSessionMissing_Throws()
    {
        var mediator = CreateMediator();
        var missingSession = SessionId.New();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mediator.Send(new ApplyMoveCommand(missingSession, ArrowId.New())));
    }
}
