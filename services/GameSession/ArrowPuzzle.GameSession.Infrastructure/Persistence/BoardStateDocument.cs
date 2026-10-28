namespace ArrowPuzzle.GameSession.Infrastructure.Persistence;

internal sealed class BoardStateDocument
{
    public int Width { get; set; }

    public int Height { get; set; }

    public List<BoardArrowDocument> Arrows { get; set; } = [];
}

internal sealed class BoardArrowDocument
{
    public string Id { get; set; } = string.Empty;

    public int HeadRow { get; set; }

    public int HeadColumn { get; set; }

    public string Direction { get; set; } = string.Empty;

    public int Length { get; set; }

    public List<BoardPathCellDocument>? Path { get; set; }
}

internal sealed class BoardPathCellDocument
{
    public int Row { get; set; }

    public int Column { get; set; }
}

internal sealed class GameSessionDocument
{
    public string SessionId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string LevelId { get; set; } = string.Empty;

    public bool IsPuzzleComplete { get; set; }

    public BoardStateDocument Board { get; set; } = new();
}
