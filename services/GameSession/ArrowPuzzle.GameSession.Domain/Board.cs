namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// Rectangular grid with non-overlapping arrows.
/// </summary>
public sealed class Board
{
    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<Arrow> Arrows { get; }

    private readonly Dictionary<Cell, ArrowId> _cellToArrow;

    private readonly Dictionary<ArrowId, Arrow> _arrowsById;

    private Board(int width, int height, IReadOnlyList<Arrow> arrows)
    {
        Width = width;
        Height = height;
        Arrows = arrows;
        _arrowsById = arrows.ToDictionary(a => a.Id);
        _cellToArrow = new Dictionary<Cell, ArrowId>(arrows.Count * 2);

        foreach (var arrow in arrows)
        {
            foreach (var cell in arrow.GetOccupiedCells())
            {
                _cellToArrow[cell] = arrow.Id;
            }
        }
    }

    public static Board Create(int width, int height, IEnumerable<Arrow> arrows)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be at least 1.");
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be at least 1.");
        }

        ArgumentNullException.ThrowIfNull(arrows);

        var arrowList = arrows as IReadOnlyList<Arrow> ?? arrows.ToList();

        if (arrowList.Count == 0)
        {
            return new Board(width, height, Array.Empty<Arrow>());
        }

        var seenIds = new HashSet<ArrowId>();
        var occupied = new Dictionary<Cell, ArrowId>();

        foreach (var arrow in arrowList)
        {
            ArgumentNullException.ThrowIfNull(arrow);

            if (!seenIds.Add(arrow.Id))
            {
                throw new ArgumentException($"Duplicate arrow id '{arrow.Id}'.", nameof(arrows));
            }

            foreach (var cell in arrow.GetOccupiedCells())
            {
                if (!cell.IsWithinBounds(height, width))
                {
                    throw new ArgumentException(
                        $"Arrow '{arrow.Id}' occupies cell ({cell.Row}, {cell.Column}) outside board {height}x{width}.",
                        nameof(arrows));
                }

                if (occupied.TryGetValue(cell, out var otherId))
                {
                    throw new ArgumentException(
                        $"Cells overlap at ({cell.Row}, {cell.Column}) between arrows '{otherId}' and '{arrow.Id}'.",
                        nameof(arrows));
                }

                occupied[cell] = arrow.Id;
            }
        }

        return new Board(width, height, arrowList);
    }

    public bool TryGetArrowIdAt(Cell cell, out ArrowId arrowId) =>
        _cellToArrow.TryGetValue(cell, out arrowId);

    public bool IsCellOccupied(Cell cell) => _cellToArrow.ContainsKey(cell);

    public Arrow GetArrow(ArrowId arrowId)
    {
        if (!_arrowsById.TryGetValue(arrowId, out var arrow))
        {
            throw new KeyNotFoundException($"Arrow '{arrowId}' is not on the board.");
        }

        return arrow;
    }

    public bool HasAnyArrows => Arrows.Count > 0;

    /// <summary>
    /// True when all arrows have been removed (win condition for a level).
    /// </summary>
    public bool IsCleared => !HasAnyArrows;

    public SlideResult Slide(ArrowId arrowId)
    {
        if (!_arrowsById.ContainsKey(arrowId))
        {
            return SlideResult.NotFound(this, arrowId);
        }

        var board = this;
        var moved = false;

        while (true)
        {
            var arrow = board.GetArrow(arrowId);
            var (deltaRow, deltaColumn) = arrow.Direction.ToDelta();
            var pathTailToHead = arrow.GetOccupiedCells();
            var head = pathTailToHead[^1];
            var headTarget = head.Offset(deltaRow, deltaColumn);

            if (IsHeadTargetBlocked(board, arrowId, headTarget))
            {
                var outcome = moved ? SlideOutcome.MovedAndStopped : SlideOutcome.BlockedAtStart;
                return SlideResult.Completed(board, outcome, arrowId);
            }

            var pathAfterStep = AdvanceTrain(pathTailToHead, headTarget);
            var remainingCells = ClipPathToOnBoardCells(pathAfterStep, board.Height, board.Width);

            if (remainingCells.Count == 0)
            {
                var withoutArrow = board.Arrows.Where(a => a.Id != arrowId).ToList();
                var clearedBoard = Create(board.Width, board.Height, withoutArrow);
                return SlideResult.Completed(clearedBoard, SlideOutcome.ExitedBoard, arrowId);
            }

            Arrow advancedArrow;
            try
            {
                advancedArrow = Arrow.FromInBoundsCells(arrow.Id, arrow.Direction, remainingCells);
            }
            catch (ArgumentException)
            {
                var outcome = moved ? SlideOutcome.MovedAndStopped : SlideOutcome.BlockedAtStart;
                return SlideResult.Completed(board, outcome, arrowId);
            }

            var updatedArrows = board.Arrows
                .Select(a => a.Id == arrowId ? advancedArrow : a)
                .ToList();

            try
            {
                board = Create(board.Width, board.Height, updatedArrows);
            }
            catch (ArgumentException)
            {
                var outcome = moved ? SlideOutcome.MovedAndStopped : SlideOutcome.BlockedAtStart;
                return SlideResult.Completed(board, outcome, arrowId);
            }

            moved = true;
        }
    }

    /// <summary>
    /// One train step: tail vacates, each segment moves into the previous cell, head advances.
    /// </summary>
    internal static IReadOnlyList<Cell> AdvanceTrain(IReadOnlyList<Cell> pathTailToHead, Cell headTarget)
    {
        if (pathTailToHead.Count == 0)
        {
            throw new ArgumentException("Path cannot be empty.", nameof(pathTailToHead));
        }

        if (pathTailToHead.Count == 1)
        {
            return new[] { headTarget };
        }

        var advanced = new Cell[pathTailToHead.Count];
        for (var index = 1; index < pathTailToHead.Count; index++)
        {
            advanced[index - 1] = pathTailToHead[index];
        }

        advanced[^1] = headTarget;
        return advanced;
    }

    internal static List<Cell> ClipPathToOnBoardCells(IReadOnlyList<Cell> path, int height, int width)
    {
        var firstOnBoard = 0;
        while (firstOnBoard < path.Count && !path[firstOnBoard].IsWithinBounds(height, width))
        {
            firstOnBoard++;
        }

        var lastOnBoard = path.Count - 1;
        while (lastOnBoard >= 0 && !path[lastOnBoard].IsWithinBounds(height, width))
        {
            lastOnBoard--;
        }

        if (firstOnBoard > lastOnBoard)
        {
            return [];
        }

        return path.Skip(firstOnBoard).Take(lastOnBoard - firstOnBoard + 1).ToList();
    }

    private static bool IsHeadTargetBlocked(Board board, ArrowId slidingArrowId, Cell headTarget)
    {
        if (!headTarget.IsWithinBounds(board.Height, board.Width))
        {
            return false;
        }

        return board.TryGetArrowIdAt(headTarget, out var occupantId) && occupantId != slidingArrowId;
    }
}
