package com.arrowpuzzle.app.game

import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.ArrowPathCellDto
import com.arrowpuzzle.app.data.dto.BoardDto
import kotlin.math.abs

object ArrowGeometry {
    data class Cell(val row: Int, val column: Int)

    fun directionDelta(direction: String): Pair<Int, Int> =
        when (direction) {
            "Up" -> -1 to 0
            "Down" -> 1 to 0
            "Left" -> 0 to -1
            "Right" -> 0 to 1
            else -> 0 to 0
        }

    /** Polyline: tail to head (head last). Straight: head first, tail last. */
    fun occupiedCells(arrow: ArrowDto): List<Cell> {
        val path = arrow.path
        if (!path.isNullOrEmpty()) {
            return path.map { Cell(it.row, it.column) }
        }

        val (backRow, backCol) = directionDelta(opposite(arrow.direction))
        return (0 until arrow.length).map { segment ->
            Cell(
                row = arrow.headRow + backRow * segment,
                column = arrow.headColumn + backCol * segment,
            )
        }
    }

    fun anySegmentInBounds(board: BoardDto, arrow: ArrowDto): Boolean =
        occupiedCells(arrow).any { cell ->
            cell.row in 0 until board.height && cell.column in 0 until board.width
        }

    fun withHead(arrow: ArrowDto, headRow: Int, headColumn: Int): ArrowDto =
        arrow.copy(headRow = headRow, headColumn = headColumn)

    fun withHead(board: BoardDto, arrowId: String, headRow: Int, headColumn: Int): BoardDto =
        board.copy(
            arrows = board.arrows.map { arrow ->
                if (arrow.id == arrowId) withHead(arrow, headRow, headColumn) else arrow
            },
        )

    /** One train step: tail vacates, body follows, head advances. */
    fun advanceTrainPath(path: List<ArrowPathCellDto>, direction: String): List<ArrowPathCellDto> {
        if (path.isEmpty()) return path
        val (dr, dc) = directionDelta(direction)
        val head = path.last()
        val headTarget = ArrowPathCellDto(row = head.row + dr, column = head.column + dc)
        if (path.size == 1) {
            return listOf(headTarget)
        }
        return path.drop(1) + headTarget
    }

    /** Server-aligned train step (clips off-board tail/head like domain). */
    fun withTrainStep(board: BoardDto, arrowId: String): BoardDto =
        board.copy(
            arrows = board.arrows.map { arrow ->
                if (arrow.id != arrowId || arrow.path.isNullOrEmpty()) {
                    arrow
                } else {
                    applyTrainStep(arrow, board.height, board.width, clipOffBoard = true)
                }
            },
        )

    /** Visual bump only: one train step without clipping (stays same length). */
    fun withTrainBumpStep(board: BoardDto, arrowId: String): BoardDto =
        board.copy(
            arrows = board.arrows.map { arrow ->
                if (arrow.id != arrowId || arrow.path.isNullOrEmpty()) {
                    arrow
                } else {
                    applyTrainStep(arrow, board.height, board.width, clipOffBoard = false)
                }
            },
        )

    private fun applyTrainStep(
        arrow: ArrowDto,
        height: Int,
        width: Int,
        clipOffBoard: Boolean,
    ): ArrowDto {
        val moved = advanceTrainPath(arrow.path.orEmpty(), arrow.direction)
        val path = if (clipOffBoard) {
            clipPathToOnBoard(moved, height, width)
        } else {
            moved
        }
        if (path.isEmpty()) {
            return arrow
        }
        val head = path.last()
        return arrow.copy(
            path = path,
            headRow = head.row,
            headColumn = head.column,
            length = path.size,
        )
    }

    private fun clipPathToOnBoard(
        path: List<ArrowPathCellDto>,
        height: Int,
        width: Int,
    ): List<ArrowPathCellDto> {
        fun inBounds(cell: ArrowPathCellDto) =
            cell.row in 0 until height && cell.column in 0 until width

        var first = 0
        while (first < path.size && !inBounds(path[first])) {
            first++
        }
        var last = path.lastIndex
        while (last >= 0 && !inBounds(path[last])) {
            last--
        }
        if (first > last) return emptyList()
        return path.subList(first, last + 1)
    }

    /**
     * Steps to animate from [from] to server [to].
     * Replays train steps until paths match — no client-side block guess (avoids extra step vs server).
     */
    fun slidePathCells(from: ArrowDto, to: ArrowDto?, board: BoardDto): List<Pair<Int, Int>> {
        if (!from.path.isNullOrEmpty()) {
            if (to != null) {
                return replayTrainSteps(from, to, board)
            }
            return simulateTrainExit(from, board)
        }

        val (dr, dc) = directionDelta(from.direction)
        if (to != null) {
            val steps = maxOf(abs(to.headRow - from.headRow), abs(to.headColumn - from.headColumn))
            if (steps == 0) return emptyList()
            var row = from.headRow
            var col = from.headColumn
            return buildList {
                repeat(steps) {
                    row += dr
                    col += dc
                    add(row to col)
                }
            }
        }

        val path = mutableListOf<Pair<Int, Int>>()
        var row = from.headRow
        var col = from.headColumn
        var current = from
        while (anySegmentInBounds(board, current)) {
            row += dr
            col += dc
            path.add(row to col)
            current = withHead(from, row, col)
        }
        return path
    }

    /** Quadros intermediários alinhados ao servidor (cada passo = um trenzinho). */
    fun replayTrainBoardFrames(board: BoardDto, arrowId: String, from: ArrowDto, to: ArrowDto): List<BoardDto> {
        if (pathsEqual(from.path.orEmpty(), to.path.orEmpty())) {
            return emptyList()
        }

        val frames = mutableListOf<BoardDto>()
        var currentBoard = board.frozen()

        repeat(200) {
            val onBoard = currentBoard.arrowById(arrowId)
            val currentPath = onBoard.path.orEmpty()
            
            // Detectar se a seta sumiu (saiu do board)
            if (currentPath.isEmpty() && from.path.isNullOrEmpty().not()) {
                // Polyline que ficou vazia = saiu do board
                return frames
            }
            
            if (pathsEqual(currentPath, to.path.orEmpty())) {
                return frames
            }

            currentBoard = withTrainStep(currentBoard, arrowId).frozen()
            frames.add(currentBoard)

            val stepped = currentBoard.arrowById(arrowId)
            val steppedPath = stepped.path.orEmpty()
            
            if (steppedPath.isEmpty() && from.path.isNullOrEmpty().not()) {
                // Saiu do board neste passo
                return frames
            }
            
            if (pathsEqual(steppedPath, to.path.orEmpty())) {
                return frames
            }
        }

        return frames
    }

    private fun replayTrainSteps(from: ArrowDto, to: ArrowDto, board: BoardDto): List<Pair<Int, Int>> =
        replayTrainBoardFrames(board, from.id, from, to).map { frame ->
            val head = frame.arrowById(from.id)
            head.headRow to head.headColumn
        }

    /** Passos visuais até a próxima casa estar bloqueada (colisão), sem alterar persistência. */
    fun simulateTrainSlideUntilBlockedFrames(board: BoardDto, arrowId: String): List<BoardDto> {
        val from = board.arrowById(arrowId)
        if (from.path.isNullOrEmpty()) {
            return emptyList()
        }

        val frames = mutableListOf<BoardDto>()
        var currentBoard = board.frozen()

        repeat(200) {
            val arrow = currentBoard.arrowById(arrowId)
            val path = arrow.path.orEmpty()
            if (path.isEmpty()) {
                return frames
            }
            val head = path.last()
            val (dr, dc) = directionDelta(arrow.direction)
            val headTarget = Cell(head.row + dr, head.column + dc)
            if (isHeadTargetBlocked(currentBoard, arrow, headTarget)) {
                return frames
            }
            currentBoard = withTrainStep(currentBoard, arrowId).frozen()
            frames.add(currentBoard)
        }

        return frames
    }

    fun simulateStraightSlideUntilBlockedFrames(board: BoardDto, arrowId: String): List<BoardDto> {
        val from = board.arrowById(arrowId)
        if (!from.path.isNullOrEmpty()) {
            return emptyList()
        }

        val frames = mutableListOf<BoardDto>()
        var row = from.headRow
        var col = from.headColumn
        val (dr, dc) = directionDelta(from.direction)
        var currentBoard = board.frozen()

        repeat(200) {
            val nextRow = row + dr
            val nextCol = col + dc
            val arrow = currentBoard.arrowById(arrowId)
            if (isHeadTargetBlocked(currentBoard, arrow, Cell(nextRow, nextCol))) {
                return frames
            }
            row = nextRow
            col = nextCol
            currentBoard = withHead(currentBoard, arrowId, row, col).frozen()
            frames.add(currentBoard)
        }

        return frames
    }

    fun simulateStraightExitBoardFrames(board: BoardDto, arrowId: String): List<BoardDto> {
        val from = board.arrowById(arrowId)
        if (!from.path.isNullOrEmpty()) {
            return emptyList()
        }

        val frames = mutableListOf<BoardDto>()
        var currentBoard = board.frozen()

        repeat(200) {
            val stepped = withTrainStepForExit(currentBoard, arrowId).frozen()
            if (stepped.arrows.none { it.id == arrowId }) {
                frames.add(stepped)
                return frames
            }
            currentBoard = stepped
            frames.add(currentBoard)
        }

        return frames
    }

    fun simulateTrainExitBoardFrames(board: BoardDto, arrowId: String): List<BoardDto> {
        val from = board.arrowById(arrowId)
        if (from.path.isNullOrEmpty()) {
            return emptyList()
        }

        val frames = mutableListOf<BoardDto>()
        var currentBoard = board.frozen()

        repeat(200) {
            val arrow = currentBoard.arrows.find { it.id == arrowId } ?: return frames
            val path = arrow.path.orEmpty()
            if (path.isEmpty()) {
                return frames
            }
            val head = path.last()
            val (dr, dc) = directionDelta(arrow.direction)
            val headTarget = Cell(head.row + dr, head.column + dc)

            if (isHeadTargetBlocked(currentBoard, arrow, headTarget)) {
                return frames
            }

            val stepped = withTrainStepForExit(currentBoard, arrowId).frozen()
            if (stepped.arrows.none { it.id == arrowId }) {
                frames.add(stepped)
                return frames
            }

            currentBoard = stepped
            frames.add(currentBoard)
        }

        return frames
    }

    /** Um passo de saída: remove a seta do tabuleiro quando não resta segmento visível. */
    fun withTrainStepForExit(board: BoardDto, arrowId: String): BoardDto {
        val arrow = board.arrows.find { it.id == arrowId } ?: return board
        if (arrow.path.isNullOrEmpty()) {
            val (dr, dc) = directionDelta(arrow.direction)
            val moved = withHead(arrow, arrow.headRow + dr, arrow.headColumn + dc)
            return if (!anySegmentInBounds(board, moved)) {
                boardWithoutArrow(board, arrowId)
            } else {
                board.copy(
                    arrows = board.arrows.map { if (it.id == arrowId) moved else it },
                )
            }
        }

        val moved = advanceTrainPath(arrow.path.orEmpty(), arrow.direction)
        val path = clipPathToOnBoard(moved, board.height, board.width)
        if (path.isEmpty()) {
            return boardWithoutArrow(board, arrowId)
        }
        val head = path.last()
        val updated = arrow.copy(
            path = path,
            headRow = head.row,
            headColumn = head.column,
            length = path.size,
        )
        return if (!anySegmentInBounds(board, updated)) {
            boardWithoutArrow(board, arrowId)
        } else {
            board.copy(
                arrows = board.arrows.map { if (it.id == arrowId) updated else it },
            )
        }
    }

    private fun boardWithoutArrow(board: BoardDto, arrowId: String): BoardDto =
        board.copy(arrows = board.arrows.filter { it.id != arrowId })

    private fun simulateTrainExit(from: ArrowDto, board: BoardDto): List<Pair<Int, Int>> {
        val steps = mutableListOf<Pair<Int, Int>>()
        var current = from

        repeat(200) {
            val path = current.path.orEmpty()
            val head = path.last()
            val (dr, dc) = directionDelta(current.direction)
            val headTarget = Cell(head.row + dr, head.column + dc)

            if (isHeadTargetBlocked(board, current, headTarget)) {
                return steps
            }

            val clipped = clipPathToOnBoard(
                advanceTrainPath(path, current.direction),
                board.height,
                board.width,
            )
            if (clipped.isEmpty()) {
                return steps
            }

            current = current.copy(
                path = clipped,
                headRow = clipped.last().row,
                headColumn = clipped.last().column,
                length = clipped.size,
            )
            steps.add(clipped.last().row to clipped.last().column)

            if (!anySegmentInBounds(board, current)) {
                return steps
            }
        }

        return steps
    }

    private fun isHeadTargetBlocked(board: BoardDto, arrow: ArrowDto, headTarget: Cell): Boolean {
        if (headTarget.row !in 0 until board.height || headTarget.column !in 0 until board.width) {
            return false
        }
        return board.arrows.any { other ->
            other.id != arrow.id &&
                occupiedCells(other).any { it.row == headTarget.row && it.column == headTarget.column }
        }
    }

    private fun pathsEqual(a: List<ArrowPathCellDto>, b: List<ArrowPathCellDto>): Boolean =
        a.size == b.size && a.indices.all { i -> a[i].row == b[i].row && a[i].column == b[i].column }

    private fun opposite(direction: String): String =
        when (direction) {
            "Up" -> "Down"
            "Down" -> "Up"
            "Left" -> "Right"
            "Right" -> "Left"
            else -> direction
        }
}
