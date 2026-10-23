using ArrowPuzzle.LevelCatalog.Domain;
using ArrowPuzzle.Protos.GameSession.V1;
using ProtoArrow = ArrowPuzzle.Protos.GameSession.V1.Arrow;
using ProtoBoard = ArrowPuzzle.Protos.GameSession.V1.Board;
using ProtoDirection = ArrowPuzzle.Protos.GameSession.V1.Direction;
using ProtoGridCell = ArrowPuzzle.Protos.GameSession.V1.GridCell;

namespace ArrowPuzzle.LevelCatalog.Api.Mapping;

internal static class LevelProtoMapper
{
    public static ProtoBoard ToProto(LevelDefinition level)
    {
        ArgumentNullException.ThrowIfNull(level);

        var board = new ProtoBoard
        {
            Width = level.Width,
            Height = level.Height,
        };

        board.Arrows.AddRange(level.Arrows.Select(ToProto));
        return board;
    }

    private static ProtoArrow ToProto(LevelArrowDefinition arrow)
    {
        var proto = new ProtoArrow
        {
            Id = arrow.ArrowId,
            HeadRow = arrow.HeadRow,
            HeadColumn = arrow.HeadColumn,
            Direction = ToProtoDirection(arrow.Direction),
            Length = arrow.Length,
        };

        if (arrow.Path is { Count: >= 2 })
        {
            proto.Path.AddRange(
                arrow.Path.Select(c => new ProtoGridCell { Row = c.Row, Column = c.Column }));
        }

        return proto;
    }

    private static ProtoDirection ToProtoDirection(string direction) =>
        direction.Trim().ToUpperInvariant() switch
        {
            "UP" => ProtoDirection.Up,
            "RIGHT" => ProtoDirection.Right,
            "DOWN" => ProtoDirection.Down,
            "LEFT" => ProtoDirection.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown arrow direction."),
        };
}
