package com.arrowpuzzle.app.game

import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.ArrowPathCellDto
import com.arrowpuzzle.app.data.dto.BoardDto

/**
 * Simula fluxo de animação para debug
 */
object AnimationFlowDebug {
    fun simulateBlockedBump(board: BoardDto, arrowId: String): String {
        val log = StringBuilder()
        log.append("=== BLOCKED_BUMP ===\n")
        
        val arrow = board.arrowById(arrowId)
        log.append("Before bump: ${arrow.id} at (${arrow.headRow},${arrow.headColumn}), path size=${arrow.path?.size}\n")
        
        val bumped = ArrowGeometry.withTrainBumpStep(board, arrowId)
        val bumpedArrow = bumped.arrowById(arrowId)
        log.append("After bump: ${bumpedArrow.id} at (${bumpedArrow.headRow},${bumpedArrow.headColumn}), path size=${bumpedArrow.path?.size}\n")
        
        val backToOrig = board.frozen()
        val origArrow = backToOrig.arrowById(arrowId)
        log.append("Back to orig: ${origArrow.id} at (${origArrow.headRow},${origArrow.headColumn}), path size=${origArrow.path?.size}\n")
        
        val match = arrow.headRow == origArrow.headRow && arrow.headColumn == origArrow.headColumn && arrow.path == origArrow.path
        log.append("Match: $match\n")
        
        return log.toString()
    }

    fun simulatePolylineMove(boardBefore: BoardDto, arrowId: String, arrowBefore: ArrowDto, arrowAfter: ArrowDto): String {
        val log = StringBuilder()
        log.append("=== POLYLINE_MOVE ===\n")
        log.append("Before: (${arrowBefore.headRow},${arrowBefore.headColumn}), path=${arrowBefore.path?.size}\n")
        log.append("After (server): (${arrowAfter.headRow},${arrowAfter.headColumn}), path=${arrowAfter.path?.size}\n")
        
        val frames = ArrowGeometry.replayTrainBoardFrames(boardBefore, arrowId, arrowBefore, arrowAfter)
        log.append("Animation frames: ${frames.size}\n")
        
        if (frames.isEmpty()) {
            log.append("WARNING: No frames generated!\n")
        } else {
            frames.forEachIndexed { idx, frame ->
                try {
                    val frameArrow = frame.arrowById(arrowId)
                    log.append("  Frame $idx: (${frameArrow.headRow},${frameArrow.headColumn}), path=${frameArrow.path?.size}\n")
                } catch (e: Exception) {
                    log.append("  Frame $idx: ERROR - ${e.message}\n")
                }
            }
        }
        
        return log.toString()
    }
}
