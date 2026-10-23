namespace ArrowPuzzle.PlayerProgress.Domain;

public sealed class PlayerProgressState
{
    public string UserId { get; }

    public IReadOnlyList<string> CompletedLevelIds { get; }

    public PlayerProgressState(string userId, IReadOnlyList<string> completedLevelIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        UserId = userId;
        CompletedLevelIds = completedLevelIds ?? Array.Empty<string>();
    }

    public PlayerProgressState WithLevelCompleted(string levelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(levelId);
        var normalized = levelId.Trim();

        if (CompletedLevelIds.Any(id => string.Equals(id, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return this;
        }

        var updated = CompletedLevelIds.ToList();
        updated.Add(normalized);
        return new PlayerProgressState(UserId, updated);
    }
}
