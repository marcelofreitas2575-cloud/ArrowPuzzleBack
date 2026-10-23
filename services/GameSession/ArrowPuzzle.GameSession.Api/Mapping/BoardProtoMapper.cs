using ArrowPuzzle.GameSession.Domain;
using DomainArrow = ArrowPuzzle.GameSession.Domain.Arrow;
using DomainBoard = ArrowPuzzle.GameSession.Domain.Board;
using DomainDirection = ArrowPuzzle.GameSession.Domain.Direction;
using DomainSlideOutcome = ArrowPuzzle.GameSession.Domain.SlideOutcome;
using ProtoArrow = ArrowPuzzle.Protos.GameSession.V1.Arrow;
using ProtoBoard = ArrowPuzzle.Protos.GameSession.V1.Board;
using ProtoDirection = ArrowPuzzle.Protos.GameSession.V1.Direction;
using ProtoGridCell = ArrowPuzzle.Protos.GameSession.V1.GridCell;
using ProtoSlideOutcome = ArrowPuzzle.Protos.GameSession.V1.SlideOutcome;

namespace ArrowPuzzle.GameSession.Api.Mapping;

internal static class BoardProtoMapper
{
    public static DomainBoard ToDomain(ProtoBoard proto)
    {
        ArgumentNullException.ThrowIfNull(proto);

        var arrows = proto.Arrows.Select(ToDomain).ToList();
        return DomainBoard.Create(proto.Width, proto.Height, arrows);
    }

    public static ProtoBoard ToProto(DomainBoard board)
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

    private static DomainArrow ToDomain(ProtoArrow proto)
    {
        var id = ArrowId.From(proto.Id);
        var direction = ToDomain(proto.Direction);

        if (proto.Path.Count >= 2)
        {
            var path = proto.Path.Select(c => new Cell(c.Row, c.Column)).ToList();
            return DomainArrow.CreatePolyline(id, direction, path);
        }

        return DomainArrow.Create(
            id,
            new Cell(proto.HeadRow, proto.HeadColumn),
            direction,
            proto.Length);
    }

    private static ProtoArrow ToProto(DomainArrow arrow)
    {
        var proto = new ProtoArrow
        {
            Id = arrow.Id.ToString(),
            HeadRow = arrow.Head.Row,
            HeadColumn = arrow.Head.Column,
            Direction = ToProto(arrow.Direction),
            Length = arrow.Length,
        };

        if (arrow.IsPolyline)
        {
            proto.Path.AddRange(
                arrow.GetOccupiedCells().Select(c => new ProtoGridCell { Row = c.Row, Column = c.Column }));
        }

        return proto;
    }

    private static DomainDirection ToDomain(ProtoDirection direction) =>
        direction switch
        {
            ProtoDirection.Up => DomainDirection.Up,
            ProtoDirection.Right => DomainDirection.Right,
            ProtoDirection.Down => DomainDirection.Down,
            ProtoDirection.Left => DomainDirection.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Direction is required."),
        };

    private static ProtoDirection ToProto(DomainDirection direction) =>
        direction switch
        {
            DomainDirection.Up => ProtoDirection.Up,
            DomainDirection.Right => ProtoDirection.Right,
            DomainDirection.Down => ProtoDirection.Down,
            DomainDirection.Left => ProtoDirection.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };

    public static ProtoSlideOutcome ToProto(DomainSlideOutcome outcome) =>
        outcome switch
        {
            DomainSlideOutcome.ArrowNotFound => ProtoSlideOutcome.ArrowNotFound,
            DomainSlideOutcome.BlockedAtStart => ProtoSlideOutcome.BlockedAtStart,
            DomainSlideOutcome.MovedAndStopped => ProtoSlideOutcome.MovedAndStopped,
            DomainSlideOutcome.ExitedBoard => ProtoSlideOutcome.ExitedBoard,
            _ => ProtoSlideOutcome.Unspecified,
        };
}
