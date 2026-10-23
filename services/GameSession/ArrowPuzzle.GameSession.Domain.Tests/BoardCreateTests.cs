namespace ArrowPuzzle.GameSession.Domain.Tests;

public class BoardCreateTests
{
    [Fact]
    public void Create_EmptyBoard_HasNoArrowsAndIsAlreadyComplete()
    {
        var board = Board.Create(3, 3, Array.Empty<Arrow>());

        Assert.False(board.HasAnyArrows);
        Assert.True(board.IsCleared);
        Assert.True(PuzzleRules.IsPuzzleComplete(board));
    }

    [Fact]
    public void Create_OverlappingArrows_Throws()
    {
        var id1 = ArrowId.New();
        var id2 = ArrowId.New();
        var a = Arrow.Create(id1, new Cell(0, 1), Direction.Right, length: 2);
        var b = Arrow.Create(id2, new Cell(0, 1), Direction.Down, length: 1);

        var ex = Assert.Throws<ArgumentException>(() => Board.Create(4, 4, new[] { a, b }));
        Assert.Contains("overlap", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ArrowOutsideBounds_Throws()
    {
        var arrow = Arrow.Create(ArrowId.New(), new Cell(0, 3), Direction.Right, length: 2);

        Assert.Throws<ArgumentException>(() => Board.Create(3, 3, new[] { arrow }));
    }
}
