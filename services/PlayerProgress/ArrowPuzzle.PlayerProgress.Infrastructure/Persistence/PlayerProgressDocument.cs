namespace ArrowPuzzle.PlayerProgress.Infrastructure.Persistence;

internal sealed class PlayerProgressDocument
{
    public string UserId { get; set; } = string.Empty;

    public List<string> CompletedLevelIds { get; set; } = [];
}
