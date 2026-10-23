package com.arrowpuzzle.app.game

import org.junit.Assert.assertEquals
import org.junit.Test

class LevelProgressionTest {
    @Test
    fun nextPlayableLevelId_startsAt001WhenEmpty() {
        assertEquals("001", LevelProgression.nextPlayableLevelId(emptyList()))
    }

    @Test
    fun nextPlayableLevelId_skipsCompletedInOrder() {
        assertEquals("003", LevelProgression.nextPlayableLevelId(listOf("001", "002")))
    }

    @Test
    fun nextLevelAfter_incrementsNumericId() {
        assertEquals("003", LevelProgression.nextLevelAfter("002"))
    }
}
