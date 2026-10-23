package com.arrowpuzzle.app.ui

import androidx.compose.ui.geometry.Offset
import com.arrowpuzzle.app.game.ArrowGeometryTest
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Test

class ArrowHitTestingTest {
    private val cellPx = 28f
    private val step = 28f

    @Test
    fun tapBetweenCells_hitsStraightArrow() {
        val arrow = ArrowGeometryTest.createArrow(
            id = "a1",
            headRow = 2,
            headColumn = 4,
            direction = "Right",
            length = 3,
        )
        val board = ArrowGeometryTest.createBoard().copy(arrows = listOf(arrow))
        val midSegment = Offset(2 * step + cellPx / 2f, 2 * step + cellPx / 2f)
        assertNotNull(ArrowHitTesting.findArrowAt(midSegment, board, step, cellPx))
    }

    @Test
    fun tapFarFromArrow_misses() {
        val arrow = ArrowGeometryTest.createArrow(
            id = "a1",
            headRow = 0,
            headColumn = 0,
            direction = "Down",
            length = 2,
        )
        val board = ArrowGeometryTest.createBoard().copy(arrows = listOf(arrow))
        val far = Offset(7 * step, 7 * step)
        assertNull(ArrowHitTesting.findArrowAt(far, board, step, cellPx))
    }

    @Test
    fun picksClosestWhenTwoNear() {
        val a = ArrowGeometryTest.createArrow("a", 1, 1, "Right", 2)
        val b = ArrowGeometryTest.createArrow("b", 3, 1, "Right", 2)
        val board = ArrowGeometryTest.createBoard().copy(arrows = listOf(a, b))
        val onA = Offset(1 * step + cellPx / 2f, 1 * step + cellPx / 2f)
        assertEquals("a", ArrowHitTesting.findArrowAt(onA, board, step, cellPx)?.id)
    }
}
