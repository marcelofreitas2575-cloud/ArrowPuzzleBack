package com.arrowpuzzle.app.progress

import android.content.Context
import androidx.core.content.edit

/** Cache local do último nível jogável (espelho do servidor; ver [PlayerProgressRepository]). */
class LocalProgressStore(context: Context) {
    private val prefs =
        context.applicationContext.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    fun loadCurrentLevelId(): String =
        prefs.getString(KEY_CURRENT_LEVEL, DEFAULT_LEVEL_ID) ?: DEFAULT_LEVEL_ID

    fun saveCurrentLevelId(levelId: String) {
        prefs.edit { putString(KEY_CURRENT_LEVEL, levelId.trim()) }
    }

    companion object {
        private const val PREFS_NAME = "arrow_puzzle_local_progress"
        private const val KEY_CURRENT_LEVEL = "current_level_id"
        private const val DEFAULT_LEVEL_ID = "001"
    }
}
