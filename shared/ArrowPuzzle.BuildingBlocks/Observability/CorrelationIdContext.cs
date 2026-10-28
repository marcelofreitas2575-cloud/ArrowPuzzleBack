namespace ArrowPuzzle.BuildingBlocks.Observability;

public static class CorrelationIdContext
{
    public const string HeaderName = "X-Correlation-Id";
    public const string LogPropertyName = "CorrelationId";

    private static readonly AsyncLocal<string?> Current = new();

    public static string? CurrentId
    {
        get => Current.Value;
        set => Current.Value = value;
    }
}
