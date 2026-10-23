package com.arrowpuzzle.app.ui

import androidx.compose.ui.geometry.Offset
import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.BoardDto
import com.arrowpuzzle.app.game.ArrowGeometry
import kotlin.math.hypot

internal object ArrowHitTesting {
    fun hitThresholdPx(cellPx: Float): Float = cellPx * 0.55f

    fun findArrowAt(
        tap: Offset,
        board: BoardDto,
        step: Float,
        cellPx: Float,
    ): ArrowDto? {
        val threshold = hitThresholdPx(cellPx)
        return board.arrows
            .mapNotNull { arrow ->
                val d = distanceToArrow(tap, arrow, board, step, cellPx)
                if (d <= threshold) arrow to d else null
            }
            .minByOrNull { it.second }
            ?.first
    }

    private fun distanceToArrow(
        tap: Offset,
        arrow: ArrowDto,
        board: BoardDto,
        step: Float,
        cellPx: Float,
    ): Float {
        val cells = ArrowGeometry.occupiedCells(arrow).filterInBoard(board)
        if (cells.isEmpty()) return Float.MAX_VALUE

        val polyline = !arrow.path.isNullOrEmpty()
        val centers = cells.map { ArrowPathDrawing.cellCenter(it.column, it.row, step, cellPx) }
        val pathPoints = if (polyline) centers else centers.asReversed()

        var best = Float.MAX_VALUE
        for (i in 0 until pathPoints.size - 1) {
            best = minOf(best, distancePointToSegment(tap, pathPoints[i], pathPoints[i + 1]))
        }

        val (dx, dy) = directionUnit(arrow.direction)
        val head = if (polyline) centers.last() else centers.first()
        val tipLead = cellPx * 0.30f
        val tip = Offset(head.x + dx * tipLead, head.y + dy * tipLead)
        best = minOf(best, distancePointToSegment(tap, head, tip))
        best = minOf(best, hypot(tap.x - tip.x, tap.y - tip.y))

        return best
    }

    private fun distancePointToSegment(p: Offset, a: Offset, b: Offset): Float {
        val abx = b.x - a.x
        val aby = b.y - a.y
        val lenSq = abx * abx + aby * aby
        if (lenSq <= 1e-6f) return hypot(p.x - a.x, p.y - a.y)
        val t = ((p.x - a.x) * abx + (p.y - a.y) * aby) / lenSq
        val clamped = t.coerceIn(0f, 1f)
        val projX = a.x + clamped * abx
        val projY = a.y + clamped * aby
        return hypot(p.x - projX, p.y - projY)
    }

    private fun directionUnit(direction: String): Pair<Float, Float> =
        when (direction) {
            "Up" -> 0f to -1f
            "Down" -> 0f to 1f
            "Left" -> -1f to 0f
            "Right" -> 1f to 0f
            else -> 0f to 0f
        }
}
