namespace ArrowPuzzle.GameSession.Domain;

public enum SlideOutcome
{
    ArrowNotFound = 0,
    BlockedAtStart = 1,
    MovedAndStopped = 2,
    ExitedBoard = 3,
}
