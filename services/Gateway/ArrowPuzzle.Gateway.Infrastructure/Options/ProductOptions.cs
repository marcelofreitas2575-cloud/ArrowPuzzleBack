namespace ArrowPuzzle.Gateway.Infrastructure.Options;

public sealed class ProductOptions
{
    public const string SectionName = "Product";

    /// <summary>Título exibido na UI do app (configurável via env, ex. Product__DisplayName).</summary>
    public string DisplayName { get; set; } = "Arrow Puzzle";
}
