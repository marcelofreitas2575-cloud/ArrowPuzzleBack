using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.LevelCatalog.Domain;

namespace ArrowPuzzle.LevelCatalog.Infrastructure.Levels;

internal static class LevelDefinitionBoardMapper
{
    public static Board ToDomainBoard(LevelDefinition level)
    {
        var arrows = level.Arrows.Select(a =>
        {
            var id = ArrowId.From(a.ArrowId);
            var direction = Enum.Parse<Direction>(a.Direction, ignoreCase: true);
            if (a.Path is { Count: >= 2 })
            {
                var cells = a.Path.Select(p => new Cell(p.Row, p.Column)).ToList();
                return Arrow.CreatePolyline(id, direction, cells);
            }

            return Arrow.Create(id, new Cell(a.HeadRow, a.HeadColumn), direction, a.Length);
        }).ToList();

        return Board.Create(level.Width, level.Height, arrows);
    }
}
