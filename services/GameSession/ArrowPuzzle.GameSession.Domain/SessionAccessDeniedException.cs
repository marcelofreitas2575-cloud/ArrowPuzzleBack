namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// The authenticated user is not allowed to access this play session.
/// </summary>
public sealed class SessionAccessDeniedException : Exception
{
    public SessionAccessDeniedException(string message)
        : base(message)
    {
    }
}
