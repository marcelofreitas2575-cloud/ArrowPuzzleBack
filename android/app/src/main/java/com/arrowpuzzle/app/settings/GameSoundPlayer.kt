package com.arrowpuzzle.app.settings

import android.content.Context
import android.media.AudioManager
import android.media.ToneGenerator
import android.util.Log

class GameSoundPlayer(@Suppress("unused") context: Context) {
    private val toneGenerator: ToneGenerator? = runCatching {
        ToneGenerator(AudioManager.STREAM_MUSIC, 100)
    }.getOrElse { error ->
        Log.w(TAG, "ToneGenerator unavailable; sounds disabled.", error)
        null
    }

    fun playTap() {
        toneGenerator?.startTone(ToneGenerator.TONE_PROP_BEEP, 50)
    }

    fun playCollision() {
        toneGenerator?.startTone(ToneGenerator.TONE_PROP_NACK, 120)
    }

    fun playWin() {
        toneGenerator?.startTone(ToneGenerator.TONE_PROP_ACK, 160)
    }

    fun release() {
        toneGenerator?.release()
    }

    companion object {
        private const val TAG = "GameSoundPlayer"
    }
}
