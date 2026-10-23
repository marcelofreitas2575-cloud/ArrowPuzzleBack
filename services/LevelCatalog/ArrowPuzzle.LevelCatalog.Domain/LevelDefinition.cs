namespace ArrowPuzzle.LevelCatalog.Domain;

public sealed class LevelDefinition
{
    public LevelId Id { get; }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<LevelArrowDefinition> Arrows { get; }

    public LevelDefinition(LevelId id, int width, int height, IReadOnlyList<LevelArrowDefinition> arrows)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        Id = id;
        Width = width;
        Height = height;
        Arrows = arrows ?? throw new ArgumentNullException(nameof(arrows));
    }
}

public sealed record LevelPathCell(int Row, int Column);

public sealed record LevelArrowDefinition(
    string ArrowId,
    int HeadRow,
    int HeadColumn,
    string Direction,
    int Length,
    IReadOnlyList<LevelPathCell>? Path = null);
