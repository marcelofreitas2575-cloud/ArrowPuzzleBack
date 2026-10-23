using ArrowPuzzle.GameSession.Application.Sessions.ApplyMove;
using ArrowPuzzle.GameSession.Application.Sessions.CreateSession;
using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.GameSession.Application;
using ArrowPuzzle.GameSession.Application.Ports;
using ArrowPuzzle.GameSession.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArrowPuzzle.GameSession.Application.Tests;

public class Level001ApplyMoveTests
{
    public static IEnumerable<object[]> ArrowIds =>
        new[]
        {
            new object[] { "a0000001-0000-4000-8000-000000000001" },
            new object[] { "a0000002-0000-4000-8000-000000000002" },
            new object[] { "a0000003-0000-4000-8000-000000000003" },
            new object[] { "a0000004-0000-4000-8000-000000000004" },
            new object[] { "a0000005-0000-4000-8000-000000000005" },
            new object[] { "a0000006-0000-4000-8000-000000000006" },
        };

    [Theory]
    [MemberData(nameof(ArrowIds))]
    public async Task ApplyMove_SoloArrow_ExitsWithoutHttpFailure(string arrowIdValue)
    {
        var mediator = CreateMediator();
        var arrowId = ArrowId.From(arrowIdValue);
        var board = Board.Create(
            7,
            11,
            Level001TestBoard.Create().Arrows.Where(a => a.Id == arrowId).ToList());
        var created = await mediator.Send(new CreateSessionCommand(board));

        var move = await mediator.Send(new ApplyMoveCommand(created.SessionId, arrowId));

        Assert.Equal(SlideOutcome.ExitedBoard, move.Outcome);
        Assert.True(move.IsSuccess);
    }

    private static IMediator CreateMediator()
    {
        var services = new ServiceCollection();
        services.AddGameSessionApplication();
        services.AddSingleton<IGameSessionRepository, InMemoryGameSessionRepository>();
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }
}

internal static class Level001TestBoard
{
    internal static Board Create()
    {
        var arrows = new[]
        {
            Arrow.CreatePolyline(
                ArrowId.From("a0000001-0000-4000-8000-000000000001"),
                Direction.Right,
                Path(
                    (10, 0), (9, 0), (8, 0), (7, 0), (6, 0), (5, 0), (4, 0), (3, 0), (2, 0), (1, 0), (0, 0),
                    (0, 1), (0, 2), (0, 3), (0, 4))),
            Arrow.CreatePolyline(
                ArrowId.From("a0000002-0000-4000-8000-000000000002"),
                Direction.Down,
                Path((0, 5), (0, 6), (1, 6), (2, 6), (3, 6), (4, 6), (5, 6), (6, 6), (7, 6), (8, 6), (9, 6), (10, 6))),
            Arrow.CreatePolyline(
                ArrowId.From("a0000003-0000-4000-8000-000000000003"),
                Direction.Left,
                Path((5, 5), (4, 5), (3, 5), (2, 5), (1, 5), (1, 4), (1, 3), (1, 2))),
            Arrow.CreatePolyline(
                ArrowId.From("a0000004-0000-4000-8000-000000000004"),
                Direction.Up,
                Path((9, 3), (9, 2), (9, 1), (8, 1), (7, 1), (6, 1), (5, 1), (4, 1))),
            Arrow.CreatePolyline(
                ArrowId.From("a0000005-0000-4000-8000-000000000005"),
                Direction.Down,
                Path((7, 3), (6, 3), (5, 3), (5, 4), (6, 4))),
            Arrow.CreatePolyline(
                ArrowId.From("a0000006-0000-4000-8000-000000000006"),
                Direction.Right,
                Path((10, 4), (10, 5))),
        };

        return Board.Create(7, 11, arrows);
    }

    private static List<Cell> Path(params (int Row, int Column)[] cells) =>
        cells.Select(c => new Cell(c.Row, c.Column)).ToList();
}
