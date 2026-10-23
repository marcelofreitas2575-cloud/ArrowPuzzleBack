namespace ArrowPuzzle.LevelCatalog.Domain;

public readonly record struct LevelId
{
    public string Value { get; }

    private LevelId(string value) => Value = value;

    public static LevelId From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new LevelId(value.Trim());
    }

    public override string ToString() => Value;
}
