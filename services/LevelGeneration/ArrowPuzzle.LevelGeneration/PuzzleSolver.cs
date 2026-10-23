using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.LevelGeneration;

/// <summary>
/// Verifica solvabilidade usando o mesmo <see cref="Board.Slide"/> do jogo.
/// </summary>
public static class PuzzleSolver
{
    public sealed record SolveResult(
        bool IsSolvable,
        IReadOnlyList<ArrowId>? RemovalOrder,
        int NodesVisited);

    public static bool IsSolvable(Board board, int maxNodes = 500_000) =>
        TrySolve(board, maxNodes).IsSolvable;

    public static SolveResult TrySolve(Board board, int maxNodes = 500_000)
    {
        ArgumentNullException.ThrowIfNull(board);

        if (board.IsCleared)
        {
            return new SolveResult(true, Array.Empty<ArrowId>(), 0);
        }

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var path = new List<ArrowId>();
        var nodes = 0;

        if (Dfs(board, visited, path, ref nodes, maxNodes))
        {
            return new SolveResult(true, path.ToArray(), nodes);
        }

        return new SolveResult(false, null, nodes);
    }

    /// <summary>
    /// Confirma que a ordem informada limpa o tabuleiro (útil para validar construção reversa).
    /// </summary>
    public static bool VerifyRemovalOrder(Board board, IReadOnlyList<ArrowId> removalOrder)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(removalOrder);

        var current = board;
        foreach (var arrowId in removalOrder)
        {
            if (!current.Arrows.Any(a => a.Id == arrowId))
            {
                return false;
            }

            var slide = current.Slide(arrowId);
            if (slide.Outcome != SlideOutcome.ExitedBoard)
            {
                return false;
            }

            current = slide.Board;
        }

        return current.IsCleared;
    }

    private static bool Dfs(
        Board board,
        HashSet<string> visited,
        List<ArrowId> path,
        ref int nodes,
        int maxNodes)
    {
        if (nodes++ > maxNodes)
        {
            return false;
        }

        if (board.IsCleared)
        {
            return true;
        }

        var key = BoardSignature(board);
        if (!visited.Add(key))
        {
            return false;
        }

        foreach (var arrow in board.Arrows)
        {
            var slide = board.Slide(arrow.Id);
            if (slide.Outcome != SlideOutcome.ExitedBoard)
            {
                continue;
            }

            path.Add(arrow.Id);
            if (Dfs(slide.Board, visited, path, ref nodes, maxNodes))
            {
                return true;
            }

            path.RemoveAt(path.Count - 1);
        }

        return false;
    }

    internal static string BoardSignature(Board board)
    {
        var parts = board.Arrows
            .OrderBy(a => a.Id.Value.ToString(), StringComparer.Ordinal)
            .Select(SignatureForArrow);
        return string.Join("|", parts);
    }

    private static string SignatureForArrow(Arrow arrow)
    {
        var cells = arrow.GetOccupiedCells();
        var cellPart = string.Join(
            ",",
            cells.Select(c => $"{c.Row}:{c.Column}"));
        return $"{arrow.Id.Value}:{(int)arrow.Direction}:{cellPart}";
    }
}
