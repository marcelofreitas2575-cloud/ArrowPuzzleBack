using ArrowPuzzle.LevelCatalog.Domain;
using ArrowPuzzle.LevelGeneration;

namespace ArrowPuzzle.LevelCatalog.Infrastructure.Levels;

/// <summary>
/// Validação independente antes de publicar um nível procedural (sem JSON no catálogo).
/// </summary>
internal static class ProceduralLevelSolvabilityGuard
{
    private const int MaxSolverNodes = 2_000_000;

    public static void EnsureSolvable(LevelDefinition level)
    {
        var board = LevelDefinitionBoardMapper.ToDomainBoard(level);

        if (!PuzzleSolver.IsSolvable(board))
        {
            throw new InvalidOperationException(
                $"Level '{level.Id.Value}' failed PuzzleSolver.IsSolvable after generation.");
        }

        var solve = PuzzleSolver.TrySolve(board, maxNodes: MaxSolverNodes);
        if (!solve.IsSolvable)
        {
            throw new InvalidOperationException(
                $"Level '{level.Id.Value}' failed independent TrySolve (nodes={solve.NodesVisited}).");
        }
    }
}
