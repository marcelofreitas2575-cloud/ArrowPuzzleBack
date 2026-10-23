using ArrowPuzzle.Gateway.Application.Models;
using ArrowPuzzle.Protos.GameSession.V1;
using ProtoArrow = ArrowPuzzle.Protos.GameSession.V1.Arrow;
using ProtoBoard = ArrowPuzzle.Protos.GameSession.V1.Board;
using ProtoDirection = ArrowPuzzle.Protos.GameSession.V1.Direction;
using ProtoGridCell = ArrowPuzzle.Protos.GameSession.V1.GridCell;
using ProtoSlideOutcome = ArrowPuzzle.Protos.GameSession.V1.SlideOutcome;

namespace ArrowPuzzle.Gateway.Infrastructure.Grpc;

internal static class ProtoDtoMapper
{
    public static BoardDto ToDto(ProtoBoard board)
    {
        ArgumentNullException.ThrowIfNull(board);

        var arrows = board.Arrows.Select(ToDto).ToList();
        return new BoardDto(board.Width, board.Height, arrows);
    }

    public static ProtoBoard ToProto(BoardDto board)
    {
        ArgumentNullException.ThrowIfNull(board);

        var proto = new ProtoBoard
        {
            Width = board.Width,
            Height = board.Height,
        };
        proto.Arrows.AddRange(board.Arrows.Select(ToProto));
        return proto;
    }

    public static string ToOutcomeName(ProtoSlideOutcome outcome) =>
        outcome switch
        {
            ProtoSlideOutcome.ArrowNotFound => "ArrowNotFound",
            ProtoSlideOutcome.BlockedAtStart => "BlockedAtStart",
            ProtoSlideOutcome.MovedAndStopped => "MovedAndStopped",
            ProtoSlideOutcome.ExitedBoard => "ExitedBoard",
            _ => "Unspecified",
        };

    private static ArrowDto ToDto(ProtoArrow arrow)
    {
        IReadOnlyList<ArrowPathCellDto>? path = null;
        if (arrow.Path.Count >= 2)
        {
            path = arrow.Path.Select(c => new ArrowPathCellDto(c.Row, c.Column)).ToList();
        }

        return new ArrowDto(
            arrow.Id,
            arrow.HeadRow,
            arrow.HeadColumn,
            ToDirectionName(arrow.Direction),
            arrow.Length,
            path);
    }

    private static ProtoArrow ToProto(ArrowDto arrow)
    {
        var proto = new ProtoArrow
        {
            Id = arrow.Id,
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

    private static string ToDirectionName(ProtoDirection direction) =>
        direction switch
        {
            ProtoDirection.Up => "Up",
            ProtoDirection.Right => "Right",
            ProtoDirection.Down => "Down",
            ProtoDirection.Left => "Left",
            _ => "Unspecified",
        };

    private static ProtoDirection ToProtoDirection(string direction) =>
        direction.Trim().ToUpperInvariant() switch
        {
            "UP" => ProtoDirection.Up,
            "RIGHT" => ProtoDirection.Right,
            "DOWN" => ProtoDirection.Down,
            "LEFT" => ProtoDirection.Left,
            _ => ProtoDirection.Unspecified,
        };
}
