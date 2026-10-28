namespace ArrowPuzzle.Gateway.Application.Progress.CompleteLevel;

/// <summary>
/// Progress cannot be saved because the server has no completed win for this user, level, and session.
/// </summary>
public sealed class LevelCompletionNotVerifiedException : Exception
{
    public LevelCompletionNotVerifiedException(string message)
        : base(message)
    {
    }
}
