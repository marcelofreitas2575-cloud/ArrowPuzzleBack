using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.GameSession.Domain.Tests;

public class Level001BoardTests
{
    [Fact]
    public void Level001_SixPolylineArrows_CreateWithoutOverlap()
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

        var board = Board.Create(width: 7, height: 11, arrows);

        Assert.Equal(6, board.Arrows.Count);
    }

    private static List<Cell> Path(params (int Row, int Column)[] cells) =>
        cells.Select(c => new Cell(c.Row, c.Column)).ToList();
}
