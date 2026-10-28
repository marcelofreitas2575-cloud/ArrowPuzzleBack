namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// Active play instance: board state bound to session, owner, and level.
/// </summary>
public sealed class PlaySession
{
    public SessionId Id { get; }

    public string UserId { get; }

    public string LevelId { get; }

    public Board Board { get; }

    public bool IsPuzzleComplete { get; }

    public PlaySession(
        SessionId id,
        string userId,
        string levelId,
        Board board,
        bool isPuzzleComplete = false)
    {
        Id = id;
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        LevelId = levelId ?? throw new ArgumentNullException(nameof(levelId));
        Board = board ?? throw new ArgumentNullException(nameof(board));
        IsPuzzleComplete = isPuzzleComplete;
    }

    public PlaySession WithBoard(Board board, bool isPuzzleComplete) =>
        new(Id, UserId, LevelId, board, isPuzzleComplete);
}
