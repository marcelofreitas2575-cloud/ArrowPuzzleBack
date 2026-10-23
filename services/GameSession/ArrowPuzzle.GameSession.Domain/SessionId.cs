namespace ArrowPuzzle.GameSession.Domain;

public readonly record struct SessionId
{
    public Guid Value { get; }

    private SessionId(Guid value) => Value = value;

    public static SessionId New() => new(Guid.NewGuid());

    public static SessionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Session id cannot be empty.", nameof(value));
        }

        return new SessionId(value);
    }

    public static SessionId From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (!Guid.TryParse(value, out var guid))
        {
            throw new FormatException("Session id must be a valid GUID.");
        }

        return From(guid);
    }

    public override string ToString() => Value.ToString("D");
}
