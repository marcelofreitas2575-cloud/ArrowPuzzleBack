using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.LevelGeneration;

/// <summary>
/// Resultado offline do gerador (ainda não integrado ao Level Catalog).
/// </summary>
public sealed record GeneratedLevel(
    Board Board,
    /// <summary>Ordem em que as setas foram colocadas (última a sair na solução = primeira aqui).</summary>
    IReadOnlyList<ArrowId> ConstructionOrder,
    /// <summary>Ordem de remoção garantida pela construção reversa (primeira a sair no jogo).</summary>
    IReadOnlyList<ArrowId> GuaranteedRemovalOrder);
