using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.LevelGeneration;

namespace ArrowPuzzle.LevelGeneration.Tests;

public class PuzzleSolverTests
{
    [Fact]
    public void EmptyBoard_IsSolvable()
    {
        var board = Board.Create(3, 3, Array.Empty<Arrow>());
        Assert.True(PuzzleSolver.IsSolvable(board));
    }

    [Fact]
    public void SingleArrowExits_IsSolvable()
    {
        var id = ArrowId.New();
        var arrow = Arrow.Create(id, new Cell(1, 1), Direction.Right, length: 1);
        var board = Board.Create(3, 3, new[] { arrow });
        var result = PuzzleSolver.TrySolve(board);
        Assert.True(result.IsSolvable);
        Assert.NotNull(result.RemovalOrder);
        Assert.Single(result.RemovalOrder!);
    }

    [Fact]
    public void VerifyRemovalOrder_RejectsBlockedMove()
    {
        var wall = Arrow.Create(ArrowId.New(), new Cell(0, 2), Direction.Down, length: 1);
        var slider = Arrow.Create(ArrowId.New(), new Cell(0, 0), Direction.Right, length: 1);
        var board = Board.Create(4, 4, new[] { wall, slider });

        Assert.False(PuzzleSolver.VerifyRemovalOrder(board, new[] { slider.Id }));
    }
}
