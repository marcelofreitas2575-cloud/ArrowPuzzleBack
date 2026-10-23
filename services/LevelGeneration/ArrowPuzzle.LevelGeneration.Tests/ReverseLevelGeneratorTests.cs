using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.LevelGeneration;

namespace ArrowPuzzle.LevelGeneration.Tests;

public class ReverseLevelGeneratorTests
{
    private readonly ReverseLevelGenerator _generator = new();

    public static IEnumerable<object[]> TailStrategies() =>
        Enum.GetValues<TailExtensionSafetyStrategy>().Cast<object>().Select(s => new[] { s });

    [Theory]
    [MemberData(nameof(TailStrategies))]
    public void GeneratedLevel_GuaranteedOrder_ClearsBoard(TailExtensionSafetyStrategy tailSafety)
    {
        var parameters = LevelGenerationParameters.ForLevelIndex(5, seed: 42, tailSafety);
        var generated = GenerateWithRetries(parameters, maxAttempts: 30);
        Assert.NotNull(generated);

        Assert.True(
            PuzzleSolver.VerifyRemovalOrder(generated!.Board, generated.GuaranteedRemovalOrder));
        Assert.True(generated.Board.IsCleared is false);
    }

    [Theory]
    [MemberData(nameof(TailStrategies))]
    public void GeneratedLevel_IndependentSolver_Agrees(TailExtensionSafetyStrategy tailSafety)
    {
        var parameters = LevelGenerationParameters.ForLevelIndex(8, seed: 991, tailSafety);
        var generated = GenerateWithRetries(parameters, maxAttempts: 40);
        Assert.NotNull(generated);

        var solve = PuzzleSolver.TrySolve(generated!.Board, maxNodes: 2_000_000);
        Assert.True(solve.IsSolvable, $"nodes={solve.NodesVisited}");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(20)]
    public void ManySeeds_ProduceSolvableLevels(int levelIndex)
    {
        var successes = 0;
        for (var seed = 0; seed < 25; seed++)
        {
            var parameters = LevelGenerationParameters.ForLevelIndex(
                levelIndex,
                seed: seed * 17 + levelIndex,
                TailExtensionSafetyStrategy.BacktrackOnFailedExitTest);

            var generated = _generator.TryGenerate(parameters);
            if (generated is null)
            {
                continue;
            }

            Assert.True(PuzzleSolver.IsSolvable(generated.Board));
            Assert.True(PuzzleSolver.VerifyRemovalOrder(
                generated.Board,
                generated.GuaranteedRemovalOrder));
            successes++;
        }

        Assert.True(successes >= 15, $"level={levelIndex} successes={successes}/25");
    }

    [Fact]
    public void BothTailStrategies_SucceedOnSameSeedFamily()
    {
        var backtrackOk = 0;
        var reserveOk = 0;

        for (var seed = 0; seed < 40; seed++)
        {
            var common = LevelGenerationParameters.ForLevelIndex(6, seed, TailExtensionSafetyStrategy.BacktrackOnFailedExitTest);
            if (_generator.TryGenerate(common) is not null)
            {
                backtrackOk++;
            }

            var reserved = common with
            {
                TailSafety = TailExtensionSafetyStrategy.ReserveExitCorridorCells,
            };
            if (_generator.TryGenerate(reserved) is not null)
            {
                reserveOk++;
            }
        }

        Assert.True(backtrackOk >= 25);
        Assert.True(reserveOk >= 15, "ReserveExitCorridor is stricter; expect fewer but some successes");
    }

    [Fact]
    public void Level001_ManualBoard_IsSolvableBySolver()
    {
        var board = Level001SampleBoard.Create();
        var solve = PuzzleSolver.TrySolve(board, maxNodes: 5_000_000);
        Assert.True(solve.IsSolvable, $"nodes={solve.NodesVisited}");
    }

    [Fact]
    public void Scaling_IncreasesArrowCountWithLevelIndex()
    {
        var p1 = LevelGenerationParameters.ForLevelIndex(1, 0);
        var p20 = LevelGenerationParameters.ForLevelIndex(20, 0);
        var p35 = LevelGenerationParameters.ForLevelIndex(35, 0);
        Assert.True(p20.ArrowCount > p1.ArrowCount);
        Assert.True(p20.Width >= p1.Width);
        Assert.True(p20.Height >= p1.Height);
        Assert.True(p35.Width * p35.Height > p20.Width * p20.Height);
        Assert.True(p35.MaxPathLength > p20.MaxPathLength);

        var p2 = LevelGenerationParameters.ForLevelIndex(2, 0);
        Assert.True(p2.Width >= 10);
        Assert.True(p2.Height >= 14);
        Assert.True(p2.ArrowCount >= 6);
    }

    [Fact]
    public void HighLevel_GeneratedBoard_OccupiesMostCells()
    {
        var parameters = LevelGenerationParameters.ForLevelIndex(32, seed: 404, TailExtensionSafetyStrategy.BacktrackOnFailedExitTest);
        var generated = GenerateWithRetries(parameters, maxAttempts: 100);
        Assert.NotNull(generated);

        var board = generated!.Board;
        var occupied = board.Arrows.Sum(a => a.GetOccupiedCells().Count);
        var cellCount = board.Width * board.Height;
        var ratio = (double)occupied / cellCount;

        Assert.True(ratio >= 0.38, $"fill={ratio:P1} arrows={board.Arrows.Count} cells={cellCount}");
        Assert.True(board.Arrows.Count >= 25, $"arrows={board.Arrows.Count}");
    }

    private GeneratedLevel? GenerateWithRetries(LevelGenerationParameters parameters, int maxAttempts)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            var attempt = parameters with { Seed = parameters.Seed + i * 31 };
            var generated = _generator.TryGenerate(attempt);
            if (generated is not null)
            {
                return generated;
            }
        }

        return null;
    }
}

internal static class Level001SampleBoard
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
