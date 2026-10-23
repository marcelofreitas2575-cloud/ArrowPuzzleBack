namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// Stable identifier for an arrow piece on the board.
/// </summary>
public readonly record struct ArrowId
{
    public Guid Value { get; }

    private ArrowId(Guid value) => Value = value;

    public static ArrowId New() => new(Guid.NewGuid());

    public static ArrowId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Arrow id cannot be empty.", nameof(value));
        }

        return new ArrowId(value);
    }

    public static ArrowId From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!Guid.TryParse(value, out var guid))
        {
            throw new FormatException("Arrow id must be a valid GUID.");
        }

        return From(guid);
    }

    public override string ToString() => Value.ToString("D");
}
