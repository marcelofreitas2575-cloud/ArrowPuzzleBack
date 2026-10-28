namespace ArrowPuzzle.Gateway.Application.Models;

public sealed record ArrowPathCellDto(int Row, int Column);

public sealed record ArrowDto(
    string Id,
    int HeadRow,
    int HeadColumn,
    string Direction,
    int Length,
    IReadOnlyList<ArrowPathCellDto>? Path = null);

public sealed record BoardDto(
    int Width,
    int Height,
    IReadOnlyList<ArrowDto> Arrows);

public sealed record LevelDto(string LevelId, BoardDto Board);

public sealed record SessionDto(
    string SessionId,
    BoardDto Board,
    string LevelId,
    bool IsPuzzleComplete);

public sealed record ApplyMoveResultDto(
    string Outcome,
    BoardDto Board,
    bool IsSuccess,
    bool IsPuzzleComplete);

public sealed record PlayerProgressDto(string UserId, IReadOnlyList<string> CompletedLevelIds);
