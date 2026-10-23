namespace ArrowPuzzle.GameSession.Domain.Tests;

public class PuzzleCompleteTests
{
    [Fact]
    public void IsPuzzleComplete_WhenLastArrowExits_ReturnsTrue()
    {
        var id = ArrowId.New();
        var arrow = Arrow.Create(id, new Cell(0, 0), Direction.Right, length: 1);
        var board = Board.Create(2, 2, new[] { arrow });

        var result = board.Slide(id);

        Assert.True(result.IsPuzzleComplete);
        Assert.True(PuzzleRules.IsPuzzleComplete(result));
        Assert.True(result.Board.IsCleared);
    }

    [Fact]
    public void IsPuzzleComplete_WhenArrowsRemain_ReturnsFalse()
    {
        var first = Arrow.Create(ArrowId.New(), new Cell(0, 0), Direction.Right, length: 1);
        var second = Arrow.Create(ArrowId.New(), new Cell(1, 3), Direction.Left, length: 1);
        var board = Board.Create(4, 2, new[] { first, second });

        var result = board.Slide(first.Id);

        Assert.Equal(SlideOutcome.ExitedBoard, result.Outcome);
        Assert.False(result.IsPuzzleComplete);
        Assert.Single(result.Board.Arrows);
    }

    [Fact]
    public void IsPuzzleComplete_BlockedMove_DoesNotCompletePuzzle()
    {
        var blocker = Arrow.Create(ArrowId.New(), new Cell(0, 1), Direction.Down, length: 1);
        var slider = Arrow.Create(ArrowId.New(), new Cell(0, 0), Direction.Right, length: 1);
        var board = Board.Create(2, 2, new[] { blocker, slider });

        var result = board.Slide(slider.Id);

        Assert.Equal(SlideOutcome.BlockedAtStart, result.Outcome);
        Assert.False(result.IsPuzzleComplete);
        Assert.Equal(2, result.Board.Arrows.Count);
    }
}
