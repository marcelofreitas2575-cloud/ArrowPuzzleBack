namespace ArrowPuzzle.Gateway.Application.Sessions;

/// <summary>
/// The authenticated user does not own this play session.
/// </summary>
public sealed class SessionAccessDeniedException : Exception
{
    public SessionAccessDeniedException(string message)
        : base(message)
    {
    }
}
