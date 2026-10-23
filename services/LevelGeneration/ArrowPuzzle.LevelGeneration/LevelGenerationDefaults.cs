namespace ArrowPuzzle.LevelGeneration;

/// <summary>
/// Decisão de produto (2026-03-22): estratégia 3.4 Backtrack como padrão.
/// </summary>
public static class LevelGenerationDefaults
{
    public const TailExtensionSafetyStrategy DefaultTailSafety =
        TailExtensionSafetyStrategy.BacktrackOnFailedExitTest;
}
