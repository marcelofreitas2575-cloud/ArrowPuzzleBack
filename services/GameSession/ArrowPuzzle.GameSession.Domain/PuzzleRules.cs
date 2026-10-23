namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// Win and completion rules for a puzzle instance on a board.
/// </summary>
public static class PuzzleRules
{
    /// <summary>
    /// The puzzle is complete when every arrow has left the board.
    /// </summary>
    public static bool IsPuzzleComplete(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);
        return board.IsCleared;
    }

    /// <summary>
    /// After a slide, the puzzle is complete if the resulting board has no arrows.
    /// </summary>
    public static bool IsPuzzleComplete(SlideResult slideResult)
    {
        ArgumentNullException.ThrowIfNull(slideResult);
        return IsPuzzleComplete(slideResult.Board);
    }
}
