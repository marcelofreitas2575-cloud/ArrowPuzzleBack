namespace ArrowPuzzle.GameSession.Domain;

/// <summary>
/// Cardinal direction of an arrow on the grid (head points this way; slide moves forward).
/// </summary>
public enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,
}

public static class DirectionExtensions
{
    public static (int DeltaRow, int DeltaColumn) ToDelta(this Direction direction) =>
        direction switch
        {
            Direction.Up => (-1, 0),
            Direction.Right => (0, 1),
            Direction.Down => (1, 0),
            Direction.Left => (0, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };

    public static Direction Opposite(this Direction direction) =>
        direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Right => Direction.Left,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
}
