using ArrowPuzzle.LevelCatalog.Domain;
using ArrowPuzzle.LevelCatalog.Infrastructure.Levels;
using ArrowPuzzle.LevelGeneration;

namespace ArrowPuzzle.LevelCatalog.Infrastructure.Tests;

public class ProceduralLevelCatalogTests
{
    public static IEnumerable<object[]> SampleLevelIds() =>
        new[] { "001", "002", "005", "010", "015", "020", "030", "050" }.Select(id => new object[] { id });

    public static IEnumerable<object[]> LevelIndicesForBatch() =>
        Enumerable.Range(1, 40).Select(i => new object[] { i });

    [Theory]
    [MemberData(nameof(SampleLevelIds))]
    public void ProceduralCatalog_GeneratesSolvableLevels(string levelId)
    {
        var repo = new ProceduralLevelCatalogRepository(new ProceduralLevelGenerator());

        var level = repo.GetByIdAsync(LevelId.From(levelId)).GetAwaiter().GetResult();
        Assert.NotNull(level);
        Assert.Equal(levelId, level!.Id.Value, StringComparer.OrdinalIgnoreCase);

        var board = LevelDefinitionBoardMapper.ToDomainBoard(level);
        Assert.True(PuzzleSolver.IsSolvable(board));
        var solve = PuzzleSolver.TrySolve(board, maxNodes: 2_000_000);
        Assert.True(solve.IsSolvable, $"nodes={solve.NodesVisited}");
    }

    [Theory]
    [MemberData(nameof(LevelIndicesForBatch))]
    public void ProceduralCatalog_FirstFortyLevels_AreSolvable(int levelIndex)
    {
        var levelId = levelIndex.ToString("D3");
        var repo = new ProceduralLevelCatalogRepository(new ProceduralLevelGenerator());

        var level = repo.GetByIdAsync(LevelId.From(levelId)).GetAwaiter().GetResult();
        Assert.NotNull(level);

        var board = LevelDefinitionBoardMapper.ToDomainBoard(level!);
        Assert.True(PuzzleSolver.IsSolvable(board));
    }

    [Fact]
    public void ProceduralCatalog_RejectsNonNumericIds()
    {
        var repo = new ProceduralLevelCatalogRepository(new ProceduralLevelGenerator());
        Assert.Null(repo.GetByIdAsync(LevelId.From("bonus")).GetAwaiter().GetResult());
    }
}
