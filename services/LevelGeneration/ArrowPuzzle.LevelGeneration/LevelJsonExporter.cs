using System.Text.Json;
using System.Text.Json.Serialization;
using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.LevelGeneration;

/// <summary>
/// Exporta no mesmo formato de <c>services/LevelCatalog/.../levels/*.json</c>.
/// </summary>
public static class LevelJsonExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string ToJson(string levelId, Board board)
    {
        var document = new LevelExportDocument
        {
            LevelId = levelId,
            Width = board.Width,
            Height = board.Height,
            Arrows = board.Arrows.Select(MapArrow).ToList(),
        };

        return JsonSerializer.Serialize(document, JsonOptions);
    }

    private static LevelExportArrow MapArrow(Arrow arrow)
    {
        var cells = arrow.GetOccupiedCells();
        return new LevelExportArrow
        {
            ArrowId = arrow.Id.Value.ToString(),
            HeadRow = arrow.Head.Row,
            HeadColumn = arrow.Head.Column,
            Direction = arrow.Direction.ToString(),
            Length = arrow.Length,
            Path = cells
                .Select(c => new LevelExportPathCell { Row = c.Row, Column = c.Column })
                .ToList(),
        };
    }

    private sealed class LevelExportDocument
    {
        public string LevelId { get; set; } = string.Empty;

        public int Width { get; set; }

        public int Height { get; set; }

        public List<LevelExportArrow> Arrows { get; set; } = [];
    }

    private sealed class LevelExportArrow
    {
        public string ArrowId { get; set; } = string.Empty;

        public int HeadRow { get; set; }

        public int HeadColumn { get; set; }

        public string Direction { get; set; } = string.Empty;

        public int Length { get; set; }

        public List<LevelExportPathCell>? Path { get; set; }
    }

    private sealed class LevelExportPathCell
    {
        public int Row { get; set; }

        public int Column { get; set; }
    }
}
