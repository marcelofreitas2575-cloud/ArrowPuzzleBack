namespace ArrowPuzzle.GameSession.Domain.Tests;

public class BoardSlideTests
{
    [Fact]
    public void Slide_UnknownArrow_ReturnsNotFound()
    {
        var board = Board.Create(3, 3, Array.Empty<Arrow>());
        var missing = ArrowId.New();

        var result = board.Slide(missing);

        Assert.Equal(SlideOutcome.ArrowNotFound, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.Same(board, result.Board);
    }

    [Fact]
    public void Slide_BlockedAtStart_DoesNotMoveArrow()
    {
        var blocker = Arrow.Create(ArrowId.New(), new Cell(0, 2), Direction.Down, length: 1);
        var slider = Arrow.Create(ArrowId.New(), new Cell(0, 1), Direction.Right, length: 2);
        var board = Board.Create(3, 3, new[] { blocker, slider });

        var result = board.Slide(slider.Id);

        Assert.Equal(SlideOutcome.BlockedAtStart, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Board.Arrows.Count);
        Assert.Equal(slider.Head, result.Board.GetArrow(slider.Id).Head);
    }

    [Fact]
    public void Slide_ExitsBoard_RemovesArrow()
    {
        var id = ArrowId.New();
        var arrow = Arrow.Create(id, new Cell(1, 1), Direction.Right, length: 1);
        var board = Board.Create(3, 3, new[] { arrow });

        var result = board.Slide(id);

        Assert.Equal(SlideOutcome.ExitedBoard, result.Outcome);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Board.Arrows);
    }

    [Fact]
    public void Slide_MovesThenBlocked_StopsAtLastValidPosition()
    {
        var wall = Arrow.Create(ArrowId.New(), new Cell(0, 3), Direction.Down, length: 1);
        var slider = Arrow.Create(ArrowId.New(), new Cell(0, 0), Direction.Right, length: 1);
        var board = Board.Create(4, 4, new[] { wall, slider });

        var result = board.Slide(slider.Id);

        Assert.Equal(SlideOutcome.MovedAndStopped, result.Outcome);
        Assert.True(result.IsSuccess);
        var moved = result.Board.GetArrow(slider.Id);
        Assert.Equal(new Cell(0, 2), moved.Head);
    }
}
