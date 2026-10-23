using ArrowPuzzle.LevelCatalog.Domain;
using ArrowPuzzle.LevelCatalog.Infrastructure.Levels;

namespace ArrowPuzzle.LevelCatalog.Infrastructure.Tests;

public class ProceduralLevelSolvabilityGuardTests
{
    [Theory]
    [InlineData("001")]
    [InlineData("015")]
    [InlineData("040")]
    public void EnsureSolvable_AcceptsProcedurallyGeneratedLevels(string levelId)
    {
        var generator = new ProceduralLevelGenerator();
        var level = generator.TryCreate(LevelId.From(levelId));
        Assert.NotNull(level);

        var exception = Record.Exception(() => ProceduralLevelSolvabilityGuard.EnsureSolvable(level!));
        Assert.Null(exception);
    }
}
