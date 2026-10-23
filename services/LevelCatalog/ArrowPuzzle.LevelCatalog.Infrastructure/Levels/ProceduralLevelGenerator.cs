using ArrowPuzzle.LevelCatalog.Domain;
using ArrowPuzzle.LevelGeneration;

namespace ArrowPuzzle.LevelCatalog.Infrastructure.Levels;

public sealed class ProceduralLevelGenerator
{
    private const int MaxSeedAttempts = 128;

    private readonly ReverseLevelGenerator _generator = new();

    public LevelDefinition? TryCreate(LevelId levelId)
    {
        if (!LevelIndex.TryParse(levelId.Value, out var index) || index < 1)
        {
            return null;
        }

        var baseSeed = LevelGenerationParameters.DeriveSeedFromLevelId(levelId.Value);

        for (var attempt = 0; attempt < MaxSeedAttempts; attempt++)
        {
            var seed = baseSeed + attempt * 7919;
            var parameters = LevelGenerationParameters.ForLevelIndex(index, seed);

            // Níveis densos: reduz setas levemente entre tentativas até achar layout solvável.
            if (attempt > 0 && attempt % 8 == 0 && parameters.ArrowCount > 8)
            {
                parameters = parameters with { ArrowCount = parameters.ArrowCount - 1 };
            }

            var generated = _generator.TryGenerate(parameters);
            if (generated is null)
            {
                continue;
            }

            if (!PuzzleSolver.IsSolvable(generated.Board))
            {
                continue;
            }

            return BoardLevelMapper.ToLevelDefinition(levelId, generated.Board);
        }

        throw new InvalidOperationException($"Could not generate solvable level '{levelId.Value}'.");
    }
}
