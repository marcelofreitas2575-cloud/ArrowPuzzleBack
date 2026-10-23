package com.arrowpuzzle.app.ui

import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.ArrowPathCellDto
import com.arrowpuzzle.app.data.dto.BoardDto

/** Debug helper */
object BumpDebug {
    fun arrowState(arrow: ArrowDto): String =
        if (arrow.path.isNullOrEmpty()) {
            "Straight(head=${arrow.headRow},${arrow.headColumn})"
        } else {
            "Polyline(path=${arrow.path.size} cells, head=${arrow.headRow},${arrow.headColumn})"
        }

    fun boardState(board: BoardDto, arrowId: String): String? =
        board.arrows.find { it.id == arrowId }?.let { arrowState(it) }
}
