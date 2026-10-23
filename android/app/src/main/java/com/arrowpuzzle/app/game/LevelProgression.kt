package com.arrowpuzzle.app.game

object LevelProgression {
    fun formatLevelId(index: Int): String =
        index.toString().padStart(3, '0')

    /** Primeiro nível ainda não marcado como concluído (001, 002, …). */
    fun nextPlayableLevelId(completedLevelIds: Collection<String>): String {
        val completed = completedLevelIds
            .map { it.trim() }
            .filter { it.isNotEmpty() }
            .toSet()
        var index = 1
        while (formatLevelId(index) in completed) {
            index++
        }
        return formatLevelId(index)
    }

    fun nextLevelAfter(completedLevelId: String): String {
        val trimmed = completedLevelId.trim()
        val numeric = trimmed.toIntOrNull()
        return if (numeric != null) {
            formatLevelId(numeric + 1)
        } else {
            nextPlayableLevelId(listOf(trimmed))
        }
    }
}
