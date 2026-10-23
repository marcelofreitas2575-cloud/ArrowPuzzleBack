package com.arrowpuzzle.app.settings

import android.content.Context
import androidx.core.content.edit

enum class BackgroundStyle {
    /** Gradiente decorativo em tela cheia (padrão). */
    GradientDecor,
    Light,
    Dark,
    Plain,
}

data class GameSettings(
    val soundEnabled: Boolean = true,
    val arrowColorArgb: Int = DEFAULT_ARROW_COLOR,
    val backgroundStyle: BackgroundStyle = BackgroundStyle.GradientDecor,
) {
    companion object {
        const val DEFAULT_ARROW_COLOR: Int = 0xFF1F1635.toInt()
    }
}

class GameSettingsStore(context: Context) {
    private val prefs =
        context.applicationContext.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    fun load(): GameSettings =
        GameSettings(
            soundEnabled = prefs.getBoolean(KEY_SOUND, true),
            arrowColorArgb = prefs.getInt(KEY_ARROW_COLOR, GameSettings.DEFAULT_ARROW_COLOR),
            backgroundStyle = prefs.getInt(KEY_BACKGROUND, BackgroundStyle.GradientDecor.ordinal)
                .let { ordinal ->
                    BackgroundStyle.entries.getOrNull(ordinal) ?: BackgroundStyle.GradientDecor
                },
        )

    fun save(settings: GameSettings) {
        prefs.edit {
            putBoolean(KEY_SOUND, settings.soundEnabled)
            putInt(KEY_ARROW_COLOR, settings.arrowColorArgb)
            putInt(KEY_BACKGROUND, settings.backgroundStyle.ordinal)
        }
    }

    companion object {
        private const val PREFS_NAME = "arrow_puzzle_game_settings"
        private const val KEY_SOUND = "sound_enabled"
        private const val KEY_ARROW_COLOR = "arrow_color_argb"
        private const val KEY_BACKGROUND = "background_style"

        val ARROW_COLOR_OPTIONS: List<Pair<String, Int>> = listOf(
            "Roxo" to GameSettings.DEFAULT_ARROW_COLOR,
            "Roxo claro" to 0xFF9C27B0.toInt(),
            "Rosa" to 0xFFE91E63.toInt(),
            "Rosa claro" to 0xFFF48FB1.toInt(),
            "Azul" to 0xFF1565C0.toInt(),
            "Verde" to 0xFF2E7D32.toInt(),
            "Vermelho" to 0xFFC62828.toInt(),
            "Laranja" to 0xFFEF6C00.toInt(),
            "Turquesa" to 0xFF00897B.toInt(),
        )
    }
}
