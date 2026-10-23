namespace ArrowPuzzle.LevelGeneration;

public sealed record LevelGenerationParameters(
    int LevelIndex,
    int Width,
    int Height,
    int ArrowCount,
    int MinPathLength,
    int MaxPathLength,
    double BendProbability,
    int MaxCandidateAttemptsPerArrow,
    TailExtensionSafetyStrategy TailSafety,
    int Seed)
{
    /// <summary>
    /// Tabuleiro ~2× o tamanho procedural anterior; ocupação sobe a cada fase até ~80–85% das células.
    /// </summary>
    public static LevelGenerationParameters ForLevelIndex(
        int levelIndex,
        int seed,
        TailExtensionSafetyStrategy tailSafety = LevelGenerationDefaults.DefaultTailSafety)
    {
        if (levelIndex < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(levelIndex));
        }

        const int minWidth = 10;
        const int minHeight = 14;
        const int maxWidth = 24;
        const int maxHeight = 32;

        var width = Math.Clamp(minWidth + (levelIndex - 1) / 2, minWidth, maxWidth);
        var height = Math.Clamp(minHeight + levelIndex, minHeight, maxHeight);
        var cellCount = width * height;

        // Meta de células ocupadas; sobe mais a cada índice (mais setas nível a nível).
        var occupancyTarget = Math.Clamp(0.14 + levelIndex * 0.021, 0.14, 0.86);
        var estimatedCellsPerArrow = Math.Clamp(3.1 + levelIndex * 0.048, 3.1, 8.8);
        var idealArrows = (int)Math.Round(cellCount * occupancyTarget / estimatedCellsPerArrow);

        var minArrows = Math.Clamp(7 + (int)Math.Round(levelIndex * 0.85), 7, 55);
        // Teto para o gerador reverso continuar encontrando layouts solváveis.
        var stabilityCap = Math.Max(minArrows, (int)Math.Round(cellCount / (7.8 + levelIndex * 0.18)));
        // Reforço perceptível nos primeiros níveis sem quebrar geração nos índices altos.
        var earlyLevelBoost = (Math.Min(levelIndex, 18) + 1) / 2;
        var arrowCount = Math.Clamp(idealArrows + earlyLevelBoost, minArrows, stabilityCap);

        var maxPath = Math.Clamp(4 + (int)Math.Round(levelIndex * 0.55), 4, 28);
        var bend = Math.Clamp(0.14 + levelIndex * 0.007, 0.14, 0.58);
        var attemptsPerArrow = Math.Clamp(60 + levelIndex * 6 + arrowCount / 3, 60, 360);

        return new LevelGenerationParameters(
            LevelIndex: levelIndex,
            Width: width,
            Height: height,
            ArrowCount: arrowCount,
            MinPathLength: 2,
            MaxPathLength: maxPath,
            BendProbability: bend,
            MaxCandidateAttemptsPerArrow: attemptsPerArrow,
            TailSafety: tailSafety,
            Seed: seed);
    }

    public static int DeriveSeedFromLevelId(string levelId) =>
        levelId.GetHashCode(StringComparison.Ordinal) ^ 0x5F3759DF;
}
