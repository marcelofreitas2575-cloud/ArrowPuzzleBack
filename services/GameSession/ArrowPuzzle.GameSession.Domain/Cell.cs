namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// Grid coordinate (may be off-board during slide simulation; <see cref="IsWithinBounds"/> gates placement).
/// </summary>
public readonly record struct Cell
{
    public int Row { get; }

    public int Column { get; }

    public Cell(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public Cell Offset(int deltaRow, int deltaColumn) => new(Row + deltaRow, Column + deltaColumn);

    public bool IsWithinBounds(int height, int width) =>
        Row >= 0 && Row < height && Column >= 0 && Column < width;
}
