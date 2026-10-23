using ArrowPuzzle.GameSession.Domain;
using ArrowPuzzle.LevelCatalog.Domain;

namespace ArrowPuzzle.LevelCatalog.Infrastructure.Levels;

internal static class BoardLevelMapper
{
    public static LevelDefinition ToLevelDefinition(LevelId levelId, Board board)
    {
        var arrows = board.Arrows.Select(MapArrow).ToList();
        return new LevelDefinition(levelId, board.Width, board.Height, arrows);
    }

    private static LevelArrowDefinition MapArrow(Arrow arrow)
    {
        var cells = arrow.GetOccupiedCells();
        if (cells.Count >= 2)
        {
            var path = cells.Select(c => new LevelPathCell(c.Row, c.Column)).ToList();
            var head = path[^1];
            return new LevelArrowDefinition(
                arrow.Id.Value.ToString(),
                head.Row,
                head.Column,
                arrow.Direction.ToString(),
                path.Count,
                path);
        }

        return new LevelArrowDefinition(
            arrow.Id.Value.ToString(),
            arrow.Head.Row,
            arrow.Head.Column,
            arrow.Direction.ToString(),
            arrow.Length);
    }
}
