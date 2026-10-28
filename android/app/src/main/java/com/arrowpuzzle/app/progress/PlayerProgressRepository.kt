package com.arrowpuzzle.app.progress

import com.arrowpuzzle.app.data.ServiceLocator
import com.arrowpuzzle.app.data.dto.CompleteLevelRequest
import com.arrowpuzzle.app.game.LevelProgression

/**
 * Fonte da verdade: BFF → Player Progress (DynamoDB). [LocalProgressStore] só espelha o último nível jogável.
 */
class PlayerProgressRepository(
    private val localCache: LocalProgressStore,
) {
    suspend fun fetchPlayableLevelId(authorization: String): String {
        val progress = ServiceLocator.gameApi.getProgress(authorization)
        val levelId = LevelProgression.nextPlayableLevelId(progress.completedLevelIds)
        localCache.saveCurrentLevelId(levelId)
        return levelId
    }

    suspend fun completeLevelAndNextPlayable(
        authorization: String,
        completedLevelId: String,
        sessionId: String,
    ): String {
        val progress = ServiceLocator.gameApi.completeLevel(
            authorization = authorization,
            body = CompleteLevelRequest(
                levelId = completedLevelId.trim(),
                sessionId = sessionId.trim(),
            ),
        )
        val nextLevelId = LevelProgression.nextPlayableLevelId(progress.completedLevelIds)
        localCache.saveCurrentLevelId(nextLevelId)
        return nextLevelId
    }
}
