using ArrowPuzzle.GameSession.Domain;

namespace ArrowPuzzle.LevelGeneration;

/// <summary>
/// Construção reversa: coloca setas na ordem inversa da solução; cada nova seta deve sair com Slide → ExitedBoard.
/// </summary>
public sealed class ReverseLevelGenerator
{
    public GeneratedLevel? TryGenerate(LevelGenerationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var rng = new Random(parameters.Seed);
        var constructionOrder = new List<ArrowId>();
        var reserved = new HashSet<Cell>();
        IReadOnlyList<Arrow> arrows = Array.Empty<Arrow>();

        for (var placed = 0; placed < parameters.ArrowCount; placed++)
        {
            var arrow = TryPlaceOneArrow(parameters, rng, arrows, reserved, out var newReservedCells);
            if (arrow is null)
            {
                return null;
            }

            constructionOrder.Add(arrow.Id);
            arrows = MergeArrow(arrows, arrow);

            if (parameters.TailSafety == TailExtensionSafetyStrategy.ReserveExitCorridorCells)
            {
                foreach (var cell in newReservedCells)
                {
                    reserved.Add(cell);
                }
            }
        }

        var board = Board.Create(parameters.Width, parameters.Height, arrows);
        var guaranteedRemoval = constructionOrder.AsEnumerable().Reverse().ToArray();

        if (!PuzzleSolver.VerifyRemovalOrder(board, guaranteedRemoval))
        {
            return null;
        }

        return new GeneratedLevel(board, constructionOrder, guaranteedRemoval);
    }

    private static Arrow? TryPlaceOneArrow(
        LevelGenerationParameters parameters,
        Random rng,
        IReadOnlyList<Arrow> existing,
        HashSet<Cell> reserved,
        out IReadOnlyList<Cell> corridorReserved)
    {
        corridorReserved = Array.Empty<Cell>();

        for (var attempt = 0; attempt < parameters.MaxCandidateAttemptsPerArrow; attempt++)
        {
            var headRow = rng.Next(0, parameters.Height);
            var headCol = rng.Next(0, parameters.Width);
            var head = new Cell(headRow, headCol);
            var direction = (Direction)rng.Next(0, 4);
            var (dr, dc) = direction.ToDelta();
            var beforeHead = new Cell(headRow - dr, headCol - dc);

            if (!IsFreeCell(beforeHead, parameters, existing, reserved))
            {
                continue;
            }

            if (IsOccupied(head, existing) || IsOccupied(beforeHead, existing))
            {
                continue;
            }

            var path = new List<Cell> { beforeHead, head };
            var arrowId = ArrowId.New();

            if (!TryBuildArrow(parameters, existing, path, direction, arrowId, out var minimalArrow))
            {
                continue;
            }

            var extended = ExtendTail(
                parameters,
                rng,
                existing,
                reserved,
                path,
                direction,
                arrowId);

            if (extended is null)
            {
                continue;
            }

            corridorReserved = CollectExitCorridor(extended, parameters);
            return extended;
        }

        return null;
    }

    private static Arrow? ExtendTail(
        LevelGenerationParameters parameters,
        Random rng,
        IReadOnlyList<Arrow> existing,
        HashSet<Cell> reserved,
        List<Cell> pathTailToHead,
        Direction direction,
        ArrowId arrowId)
    {
        var targetLength = rng.Next(parameters.MinPathLength, parameters.MaxPathLength + 1);

        while (pathTailToHead.Count < targetLength)
        {
            var tail = pathTailToHead[0];
            var neighbors = OrthogonalNeighbors(tail)
                .Where(n => IsFreeCell(n, parameters, existing, reserved))
                .Where(n => !pathTailToHead.Contains(n))
                .OrderBy(_ => rng.Next())
                .ToList();

            if (neighbors.Count == 0)
            {
                break;
            }

            var extended = false;
            foreach (var candidate in neighbors)
            {
                if (pathTailToHead.Count >= 2)
                {
                    var bend = candidate.Row != tail.Row && candidate.Column != tail.Column;
                    if (bend && rng.NextDouble() > parameters.BendProbability)
                    {
                        continue;
                    }
                }

                pathTailToHead.Insert(0, candidate);

                if (!TryBuildArrow(parameters, existing, pathTailToHead, direction, arrowId, out var arrow))
                {
                    pathTailToHead.RemoveAt(0);
                    continue;
                }

                if (!CanExitWithExisting(parameters, existing, arrow))
                {
                    pathTailToHead.RemoveAt(0);

                    if (parameters.TailSafety == TailExtensionSafetyStrategy.BacktrackOnFailedExitTest)
                    {
                        continue;
                    }

                    return null;
                }

                extended = true;
                break;
            }

            if (!extended)
            {
                break;
            }
        }

        return TryBuildArrow(parameters, existing, pathTailToHead, direction, arrowId, out var finalArrow)
            ? finalArrow
            : null;
    }

    private static bool TryBuildArrow(
        LevelGenerationParameters parameters,
        IReadOnlyList<Arrow> existing,
        List<Cell> pathTailToHead,
        Direction direction,
        ArrowId arrowId,
        out Arrow arrow)
    {
        arrow = null!;

        if (pathTailToHead.Count < 2)
        {
            return false;
        }

        foreach (var cell in pathTailToHead)
        {
            if (!cell.IsWithinBounds(parameters.Height, parameters.Width))
            {
                return false;
            }

            if (IsOccupied(cell, existing))
            {
                return false;
            }
        }

        try
        {
            arrow = Arrow.CreatePolyline(arrowId, direction, pathTailToHead);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return CanExitWithExisting(parameters, existing, arrow);
    }

    private static bool CanExitWithExisting(
        LevelGenerationParameters parameters,
        IReadOnlyList<Arrow> existing,
        Arrow candidate)
    {
        var all = existing.Concat(new[] { candidate }).ToList();
        try
        {
            var board = Board.Create(parameters.Width, parameters.Height, all);
            var slide = board.Slide(candidate.Id);
            return slide.Outcome == SlideOutcome.ExitedBoard;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static IReadOnlyList<Arrow> MergeArrow(IReadOnlyList<Arrow> existing, Arrow arrow) =>
        existing.Concat(new[] { arrow }).ToList();

    private static bool IsFreeCell(
        Cell cell,
        LevelGenerationParameters parameters,
        IReadOnlyList<Arrow> existing,
        HashSet<Cell> reserved)
    {
        if (!cell.IsWithinBounds(parameters.Height, parameters.Width))
        {
            return false;
        }

        if (IsOccupied(cell, existing))
        {
            return false;
        }

        return !reserved.Contains(cell);
    }

    private static bool IsOccupied(Cell cell, IReadOnlyList<Arrow> arrows) =>
        arrows.Any(a => a.GetOccupiedCells().Any(c => c.Row == cell.Row && c.Column == cell.Column));

    private static IEnumerable<Cell> OrthogonalNeighbors(Cell cell)
    {
        yield return new Cell(cell.Row - 1, cell.Column);
        yield return new Cell(cell.Row + 1, cell.Column);
        yield return new Cell(cell.Row, cell.Column - 1);
        yield return new Cell(cell.Row, cell.Column + 1);
    }

    private static IReadOnlyList<Cell> CollectExitCorridor(Arrow arrow, LevelGenerationParameters parameters)
    {
        var corridor = new List<Cell>();
        var head = arrow.Head;
        var (dr, dc) = arrow.Direction.ToDelta();
        var row = head.Row;
        var col = head.Column;

        while (true)
        {
            row += dr;
            col += dc;
            var cell = new Cell(row, col);
            if (!cell.IsWithinBounds(parameters.Height, parameters.Width))
            {
                break;
            }

            corridor.Add(cell);
        }

        return corridor;
    }
}
