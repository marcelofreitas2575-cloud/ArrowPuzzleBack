namespace ArrowPuzzle.Gateway.Infrastructure.Options;

public sealed class ServiceEndpointsOptions
{
    public const string SectionName = "Services";

    public string GameSession { get; set; } = "http://localhost:5101";

    public string LevelCatalog { get; set; } = "http://localhost:5102";

    public string PlayerProgress { get; set; } = "http://localhost:5103";
}
