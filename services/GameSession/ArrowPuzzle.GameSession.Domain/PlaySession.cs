namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// Active play instance: one board state bound to a session identifier.
/// </summary>
public sealed class PlaySession
{
    public SessionId Id { get; }

    public Board Board { get; }

    public PlaySession(SessionId id, Board board)
    {
        Id = id;
        Board = board ?? throw new ArgumentNullException(nameof(board));
    }

    public PlaySession WithBoard(Board board) => new(Id, board);
}
