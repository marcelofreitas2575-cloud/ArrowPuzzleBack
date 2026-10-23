using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.GameSession.Infrastructure.Persistence;

internal static class BoardStateMapper
{
    public static BoardStateDocument ToDocument(Board board) =>
        new()
        {
            Width = board.Width,
            Height = board.Height,
            Arrows = board.Arrows.Select(ToDocument).ToList(),
        };

    public static Board ToDomain(BoardStateDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var arrows = document.Arrows.Select(ToDomain).ToList();
        return Board.Create(document.Width, document.Height, arrows);
    }

    private static BoardArrowDocument ToDocument(Arrow arrow)
    {
        var doc = new BoardArrowDocument
        {
            Id = arrow.Id.ToString(),
            HeadRow = arrow.Head.Row,
            HeadColumn = arrow.Head.Column,
            Direction = arrow.Direction.ToString(),
            Length = arrow.Length,
        };

        if (arrow.IsPolyline)
        {
            doc.Path = arrow.GetOccupiedCells()
                .Select(c => new BoardPathCellDocument { Row = c.Row, Column = c.Column })
                .ToList();
        }

        return doc;
    }

    private static Arrow ToDomain(BoardArrowDocument document)
    {
        var id = ArrowId.From(document.Id);
        var direction = ParseDirection(document.Direction);

        if (document.Path is { Count: >= 2 })
        {
            var path = document.Path.Select(c => new Cell(c.Row, c.Column)).ToList();
            return Arrow.CreatePolyline(id, direction, path);
        }

        return Arrow.Create(
            id,
            new Cell(document.HeadRow, document.HeadColumn),
            direction,
            document.Length);
    }

    private static Direction ParseDirection(string direction) =>
        direction.Trim().ToUpperInvariant() switch
        {
            "UP" => Direction.Up,
            "RIGHT" => Direction.Right,
            "DOWN" => Direction.Down,
            "LEFT" => Direction.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown direction."),
        };
}
