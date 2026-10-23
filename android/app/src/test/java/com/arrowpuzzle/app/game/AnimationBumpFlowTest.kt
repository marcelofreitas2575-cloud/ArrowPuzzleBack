package com.arrowpuzzle.app.game

import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.ArrowPathCellDto
import com.arrowpuzzle.app.data.dto.BoardDto
import org.junit.Test

class AnimationBumpFlowTest {
    private fun arrow(id: String, headRow: Int, headCol: Int, dir: String, len: Int, path: List<Pair<Int, Int>>? = null): ArrowDto =
        ArrowDto(
            id, headRow, headCol, dir, len,
            path?.map { (r, c) -> ArrowPathCellDto(r, c) }
        )

    private fun board(w: Int, h: Int, vararg arrows: ArrowDto) = BoardDto(w, h, arrows.toList())

    @Test
    fun testPolylineBumpReturnsToExactPosition() {
        println("=== TEST: Polyline Bump Returns to Exact Position ===")
        
        // Setup: large L-shaped arrow
        val initialBoard = board(
            7, 11,
            arrow("a1", 0, 4, "Right", 15, listOf(
                10 to 0, 9 to 0, 8 to 0, 7 to 0, 6 to 0, 5 to 0, 4 to 0, 3 to 0, 2 to 0, 1 to 0, 0 to 0,
                0 to 1, 0 to 2, 0 to 3, 0 to 4
            ))
        )

        val a1Before = initialBoard.arrowById("a1")
        val posBeforeStr = "(${a1Before.headRow},${a1Before.headColumn})"
        val pathBeforeStr = a1Before.path?.size ?: 0
        println("Before bump: pos=$posBeforeStr, pathSize=$pathBeforeStr")

        // Do bump
        val bumped = ArrowGeometry.withTrainBumpStep(initialBoard, "a1")
        val a1Bumped = bumped.arrowById("a1")
        val posBumpedStr = "(${a1Bumped.headRow},${a1Bumped.headColumn})"
        println("After bump: pos=$posBumpedStr")
        
        // Verify bumped position is different
        check(a1Bumped.headRow != a1Before.headRow || a1Bumped.headColumn != a1Before.headColumn) {
            "Bump should move arrow, but position didn't change!"
        }

        // Return to original (like BlockedAtStart does)
        val returnedBoard = initialBoard.frozen()
        val a1Returned = returnedBoard.arrowById("a1")
        val posReturnedStr = "(${a1Returned.headRow},${a1Returned.headColumn})"
        println("After return: pos=$posReturnedStr")

        // Verify EXACT match
        check(a1Returned.headRow == a1Before.headRow) { "Row mismatch: ${a1Returned.headRow} != ${a1Before.headRow}" }
        check(a1Returned.headColumn == a1Before.headColumn) { "Column mismatch: ${a1Returned.headColumn} != ${a1Before.headColumn}" }
        check(a1Returned.path?.size == a1Before.path?.size) { "Path size mismatch" }
        check(a1Returned.path == a1Before.path) { "Path mismatch" }
        
        println("✓ PASS: Arrow returned to exact original position")
    }

    @Test
    fun testPolylineReplayFrames() {
        println("=== TEST: Polyline Replay Frames ===")
        
        val initialBoard = board(
            7, 11,
            arrow("a1", 0, 4, "Right", 15, listOf(
                10 to 0, 9 to 0, 8 to 0, 7 to 0, 6 to 0, 5 to 0, 4 to 0, 3 to 0, 2 to 0, 1 to 0, 0 to 0,
                0 to 1, 0 to 2, 0 to 3, 0 to 4
            ))
        )

        val a1 = initialBoard.arrowById("a1")
        
        // Simulate one train step
        val stepped = ArrowGeometry.withTrainStep(initialBoard, "a1")
        val a1Stepped = stepped.arrowById("a1")
        
        println("Initial: (${a1.headRow},${a1.headColumn}), path=${a1.path?.size}")
        println("Stepped: (${a1Stepped.headRow},${a1Stepped.headColumn}), path=${a1Stepped.path?.size}")
        
        // Generate frames
        val frames = ArrowGeometry.replayTrainBoardFrames(initialBoard, "a1", a1, a1Stepped)
        println("Generated ${frames.size} frames")
        
        check(frames.isNotEmpty()) { "Should generate at least 1 frame" }
        
        frames.forEachIndexed { idx, frame ->
            val arrow = frame.arrowById("a1")
            println("  Frame $idx: (${arrow.headRow},${arrow.headColumn}), pathSize=${arrow.path?.size}")
            check(arrow.id == "a1") { "Frame $idx: Arrow ID mismatch" }
        }
        
        // Verify last frame matches target
        val lastFrame = frames.last()
        val lastArrow = lastFrame.arrowById("a1")
        check(lastArrow.headRow == a1Stepped.headRow) { "Last frame head row mismatch" }
        check(lastArrow.headColumn == a1Stepped.headColumn) { "Last frame head col mismatch" }
        
        println("✓ PASS: Replay frames generated correctly")
    }

    @Test
    fun testFrozenPreservesArrow() {
        println("=== TEST: Frozen Preserves Arrow ===")
        
        val initial = board(
            7, 11,
            arrow("a1", 5, 3, "Up", 10, listOf(5 to 3, 4 to 3, 3 to 3))
        )
        
        val a1Before = initial.arrowById("a1")
        val frozen = initial.frozen()
        val a1After = frozen.arrowById("a1")
        
        println("Before: id=${a1Before.id}, pos=(${a1Before.headRow},${a1Before.headColumn}), pathSize=${a1Before.path?.size}")
        println("After:  id=${a1After.id}, pos=(${a1After.headRow},${a1After.headColumn}), pathSize=${a1After.path?.size}")
        
        check(a1Before.id == a1After.id) { "ID changed" }
        check(a1Before.headRow == a1After.headRow) { "HeadRow changed" }
        check(a1Before.headColumn == a1After.headColumn) { "HeadColumn changed" }
        check(a1Before.path == a1After.path) { "Path changed" }
        
        // Verify it's a different reference (not same object)
        check(initial.arrows !== frozen.arrows) { "Should create new list" }
        
        println("✓ PASS: Frozen preserves arrow identity and creates new references")
    }
}
