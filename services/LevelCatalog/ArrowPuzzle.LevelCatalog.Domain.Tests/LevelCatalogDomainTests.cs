using ArrowPuzzle.LevelCatalog.Domain;

namespace ArrowPuzzle.LevelCatalog.Domain.Tests;

public class LevelCatalogDomainTests
{
    [Fact]
    public void LevelId_From_TrimsValue()
    {
        var id = LevelId.From("  007  ");
        Assert.Equal("007", id.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LevelId_From_Empty_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => LevelId.From(value));
    }

    [Fact]
    public void LevelDefinition_InvalidWidth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LevelDefinition(LevelId.From("001"), 0, 3, Array.Empty<LevelArrowDefinition>()));
    }

    [Fact]
    public void LevelDefinition_NullArrows_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LevelDefinition(LevelId.From("001"), 3, 3, null!));
    }
}
