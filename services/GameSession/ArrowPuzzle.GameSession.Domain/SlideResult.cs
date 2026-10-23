namespace ArrowPuzzle.GameSession.Domain;

public sealed class SlideResult
{
    public Board Board { get; }

    public SlideOutcome Outcome { get; }

    public ArrowId ArrowId { get; }

    private SlideResult(Board board, SlideOutcome outcome, ArrowId arrowId)
    {
        Board = board;
        Outcome = outcome;
        ArrowId = arrowId;
    }

    public static SlideResult NotFound(Board board, ArrowId arrowId) =>
        new(board, SlideOutcome.ArrowNotFound, arrowId);

    public static SlideResult Completed(Board board, SlideOutcome outcome, ArrowId arrowId) =>
        new(board, outcome, arrowId);

    public bool IsSuccess =>
        Outcome is SlideOutcome.MovedAndStopped or SlideOutcome.ExitedBoard;

    /// <summary>
    /// True when this move leaves no arrows on the board (level complete).
    /// </summary>
    public bool IsPuzzleComplete => PuzzleRules.IsPuzzleComplete(Board);
}
