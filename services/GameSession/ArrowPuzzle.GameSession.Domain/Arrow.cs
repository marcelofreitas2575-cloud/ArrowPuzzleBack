namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// Arrow on the board: straight segment or orthogonal polyline (train slide in <see cref="Direction"/>).
/// </summary>
public sealed class Arrow
{
    public ArrowId Id { get; }

    public Cell Head { get; }

    public Direction Direction { get; }

    public int Length { get; }

    public bool IsPolyline => _pathTailToHead is not null;

    private readonly Cell[]? _pathTailToHead;

    private Arrow(ArrowId id, Cell head, Direction direction, int length, Cell[]? pathTailToHead)
    {
        Id = id;
        Head = head;
        Direction = direction;
        Length = length;
        _pathTailToHead = pathTailToHead;
    }

    public static Arrow Create(ArrowId id, Cell head, Direction direction, int length)
    {
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be at least 1.");
        }

        return new Arrow(id, head, direction, length, pathTailToHead: null);
    }

    public static Arrow CreatePolyline(ArrowId id, Direction direction, IReadOnlyList<Cell> pathTailToHead)
    {
        ArgumentNullException.ThrowIfNull(pathTailToHead);

        if (pathTailToHead.Count < 2)
        {
            throw new ArgumentException("Polyline arrows need at least two cells.", nameof(pathTailToHead));
        }

        ValidateAdjacentPath(pathTailToHead);

        var head = pathTailToHead[^1];
        ValidateHeadSegmentDirection(pathTailToHead, direction);

        return new Arrow(id, head, direction, pathTailToHead.Count, pathTailToHead.ToArray());
    }

    /// <summary>
    /// Cells occupied by this arrow, from tail to head (head last).
    /// </summary>
    public IReadOnlyList<Cell> GetOccupiedCells()
    {
        if (_pathTailToHead is not null)
        {
            return _pathTailToHead;
        }

        var cells = new Cell[Length];
        cells[Length - 1] = Head;
        var (deltaRow, deltaColumn) = Direction.Opposite().ToDelta();

        for (var index = Length - 2; index >= 0; index--)
        {
            cells[index] = cells[index + 1].Offset(deltaRow, deltaColumn);
        }

        return cells;
    }

    internal static Arrow FromInBoundsCells(ArrowId id, Direction direction, IReadOnlyList<Cell> cellsTailToHead)
    {
        if (cellsTailToHead.Count < 1)
        {
            throw new ArgumentException("At least one cell is required.", nameof(cellsTailToHead));
        }

        if (cellsTailToHead.Count == 1)
        {
            return Create(id, cellsTailToHead[0], direction, length: 1);
        }

        if (IsStraightCollinear(cellsTailToHead, direction))
        {
            var head = cellsTailToHead[^1];
            return Create(id, head, direction, cellsTailToHead.Count);
        }

        return CreatePolyline(id, direction, cellsTailToHead);
    }

    private static void ValidateAdjacentPath(IReadOnlyList<Cell> pathTailToHead)
    {
        for (var index = 1; index < pathTailToHead.Count; index++)
        {
            var previous = pathTailToHead[index - 1];
            var current = pathTailToHead[index];
            var rowDelta = Math.Abs(current.Row - previous.Row);
            var columnDelta = Math.Abs(current.Column - previous.Column);

            if (rowDelta + columnDelta != 1)
            {
                throw new ArgumentException(
                    $"Path must be orthogonal and contiguous; gap between ({previous.Row}, {previous.Column}) and ({current.Row}, {current.Column}).",
                    nameof(pathTailToHead));
            }
        }
    }

    private static void ValidateHeadSegmentDirection(IReadOnlyList<Cell> pathTailToHead, Direction direction)
    {
        var beforeHead = pathTailToHead[^2];
        var head = pathTailToHead[^1];
        var (deltaRow, deltaColumn) = direction.ToDelta();

        if (head.Row - beforeHead.Row != deltaRow || head.Column - beforeHead.Column != deltaColumn)
        {
            throw new ArgumentException(
                $"Last path segment must point {direction}.",
                nameof(pathTailToHead));
        }
    }

    private static bool IsStraightCollinear(IReadOnlyList<Cell> cellsTailToHead, Direction direction)
    {
        var (backRow, backColumn) = direction.Opposite().ToDelta();

        for (var index = cellsTailToHead.Count - 2; index >= 0; index--)
        {
            var current = cellsTailToHead[index];
            var next = cellsTailToHead[index + 1];

            if (current.Row + backRow != next.Row || current.Column + backColumn != next.Column)
            {
                return false;
            }
        }

        var head = cellsTailToHead[^1];
        var tail = cellsTailToHead[0];

        return direction switch
        {
            Direction.Up or Direction.Down => tail.Column == head.Column,
            Direction.Left or Direction.Right => tail.Row == head.Row,
            _ => false,
        };
    }

}
