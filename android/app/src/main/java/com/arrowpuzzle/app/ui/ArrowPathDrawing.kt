package com.arrowpuzzle.app.ui

import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.StrokeJoin
import androidx.compose.ui.graphics.drawscope.DrawScope
import androidx.compose.ui.graphics.drawscope.Stroke
import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.BoardDto
import com.arrowpuzzle.app.game.ArrowGeometry

internal object ArrowPathDrawing {
    fun DrawScope.drawArrowPath(
        arrow: ArrowDto,
        board: BoardDto,
        step: Float,
        cellPx: Float,
        arrowInk: Color,
    ) {
        val cells = ArrowGeometry.occupiedCells(arrow).filterInBoard(board)
        if (cells.isEmpty()) return

        val strokeWidth = cellPx * 0.132f
        val stroke = Stroke(
            width = strokeWidth,
            cap = StrokeCap.Butt,
            join = StrokeJoin.Round,
        )

        val polyline = !arrow.path.isNullOrEmpty()
        val centers = cells.map { cellCenter(it.column, it.row, step, cellPx) }
        val head = if (polyline) centers.last() else centers.first()
        val (dx, dy) = directionUnit(arrow.direction)
        val tipLead = cellPx * 0.34f

        val tip = Offset(head.x + dx * tipLead, head.y + dy * tipLead)
        val tipBase = Offset(tip.x - dx * strokeWidth * 2.0f, tip.y - dy * strokeWidth * 2.0f)

        val pathFromTailToHead = if (polyline) centers else centers.asReversed()
        val bodyPath = Path().apply {
            moveTo(pathFromTailToHead.first().x, pathFromTailToHead.first().y)
            pathFromTailToHead.drop(1).forEach { lineTo(it.x, it.y) }
            lineTo(tipBase.x, tipBase.y)
        }
        drawPath(bodyPath, color = arrowInk, style = stroke)
        drawArrowHead(tip, arrow.direction, arrowInk, strokeWidth, cellPx)
    }

    private val guideLineColor = Color(0xFF2196F3).copy(alpha = 0.28f)

    fun DrawScope.drawDirectionGuides(
        board: BoardDto,
        step: Float,
        cellPx: Float,
        boardWidthPx: Float,
        boardHeightPx: Float,
    ) {
        val guideStrokeWidth = cellPx * 0.045f

        board.arrows.forEach { arrow ->
            val cells = ArrowGeometry.occupiedCells(arrow).filterInBoard(board)
            if (cells.isEmpty()) return@forEach

            val polyline = !arrow.path.isNullOrEmpty()
            val centers = cells.map { cellCenter(it.column, it.row, step, cellPx) }
            val head = if (polyline) centers.last() else centers.first()
            val (dx, dy) = directionUnit(arrow.direction)
            if (dx == 0f && dy == 0f) return@forEach

            val start = Offset(head.x + dx * cellPx * 0.22f, head.y + dy * cellPx * 0.22f)
            val end = rayToBoardEdge(start, dx, dy, boardWidthPx, boardHeightPx)
            drawLine(
                color = guideLineColor,
                start = start,
                end = end,
                strokeWidth = guideStrokeWidth,
            )
        }
    }

    private fun rayToBoardEdge(
        origin: Offset,
        dx: Float,
        dy: Float,
        boardWidthPx: Float,
        boardHeightPx: Float,
    ): Offset {
        var bestT = Float.POSITIVE_INFINITY
        if (dx > 0f) bestT = minOf(bestT, (boardWidthPx - origin.x) / dx)
        if (dx < 0f) bestT = minOf(bestT, (0f - origin.x) / dx)
        if (dy > 0f) bestT = minOf(bestT, (boardHeightPx - origin.y) / dy)
        if (dy < 0f) bestT = minOf(bestT, (0f - origin.y) / dy)
        if (!bestT.isFinite() || bestT <= 0f) return origin
        return Offset(origin.x + dx * bestT, origin.y + dy * bestT)
    }

    private fun DrawScope.drawArrowHead(
        tip: Offset,
        direction: String,
        color: Color,
        strokeWidth: Float,
        cellPx: Float,
    ) {
        val (dx, dy) = directionUnit(direction)
        val length = maxOf(strokeWidth * 5.2f, cellPx * 0.22f)
        val halfWidth = maxOf(strokeWidth * 1.55f, cellPx * 0.095f)
        val base = Offset(tip.x - dx * length, tip.y - dy * length)
        val perpX = -dy
        val perpY = dx
        val path = Path().apply {
            moveTo(tip.x, tip.y)
            lineTo(base.x + perpX * halfWidth, base.y + perpY * halfWidth)
            lineTo(base.x - perpX * halfWidth, base.y - perpY * halfWidth)
            close()
        }
        drawPath(path, color = color)
    }

    fun cellCenter(column: Int, row: Int, step: Float, cellPx: Float): Offset =
        Offset(column * step + cellPx / 2f, row * step + cellPx / 2f)

    private fun directionUnit(direction: String): Pair<Float, Float> =
        when (direction) {
            "Up" -> 0f to -1f
            "Down" -> 0f to 1f
            "Left" -> -1f to 0f
            "Right" -> 1f to 0f
            else -> 0f to 0f
        }
}

internal fun List<ArrowGeometry.Cell>.filterInBoard(board: BoardDto): List<ArrowGeometry.Cell> =
    filter { cell ->
        cell.row in 0 until board.height && cell.column in 0 until board.width
    }
