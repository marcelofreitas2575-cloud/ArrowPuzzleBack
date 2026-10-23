using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.GameSession.Domain.Tests;

public class BoardTrainSlideTests
{
    [Fact]
    public void Slide_PolylineDown_ExitsBoardThroughCorner()
    {
        var id = ArrowId.New();
        var arrow = Arrow.CreatePolyline(
            id,
            Direction.Down,
            Path((0, 0), (0, 1), (1, 1)));
        var board = Board.Create(width: 3, height: 3, new[] { arrow });

        var result = board.Slide(id);

        Assert.Equal(SlideOutcome.ExitedBoard, result.Outcome);
        Assert.Empty(result.Board.Arrows);
    }

    [Fact]
    public void AdvanceTrain_ShiftsTailIntoBody()
    {
        var path = Path((0, 0), (0, 1), (1, 1));
        var advanced = Board.AdvanceTrain(path, new Cell(2, 1));

        Assert.Equal(new Cell(0, 1), advanced[0]);
        Assert.Equal(new Cell(1, 1), advanced[1]);
        Assert.Equal(new Cell(2, 1), advanced[2]);
    }

    [Fact]
    public void AdvanceTrain_RemovesHorizontalTailWhenHeadMovesDown()
    {
        var path = Path((0, 1), (0, 2), (1, 2));
        var advanced = Board.AdvanceTrain(path, new Cell(2, 2));

        Assert.Equal(3, advanced.Count);
        Assert.Equal(new Cell(0, 2), advanced[0]);
        Assert.DoesNotContain(new Cell(0, 1), advanced);
    }

    private static List<Cell> Path(params (int Row, int Column)[] cells) =>
        cells.Select(c => new Cell(c.Row, c.Column)).ToList();
}
