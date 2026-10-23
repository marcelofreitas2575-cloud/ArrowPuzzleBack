namespace ArrowPuzzle.LevelGeneration;

/// <summary>
/// Seção 3.4 do desenho: como evitar deadlock ao estender a cauda durante construção reversa.
/// </summary>
public enum TailExtensionSafetyStrategy
{
    /// <summary>
    /// Estilo Snek!: tenta estender a cauda; se o Slide deixa de dar ExitedBoard, desfaz e tenta outra célula.
    /// Menos células “reservadas”, tabuleiro mais cheio.
    /// </summary>
    BacktrackOnFailedExitTest = 0,

    /// <summary>
    /// Marca o corredor de saída (da cabeça até a borda) como reservado; novas caudas não ocupam essas células.
    /// Tabuleiro tende a ficar mais vazio, construção mais conservadora.
    /// </summary>
    ReserveExitCorridorCells = 1,
}
