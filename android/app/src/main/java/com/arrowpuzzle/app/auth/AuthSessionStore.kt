package com.arrowpuzzle.app.auth

import android.content.Context
import android.content.SharedPreferences
import androidx.core.content.edit

data class StoredAuthSession(
    val accessToken: String,
    val refreshToken: String?,
    val expiresAtEpochMs: Long,
    val clientId: String,
)

class AuthSessionStore(context: Context) {
    private val prefs: SharedPreferences =
        context.applicationContext.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    fun save(session: StoredAuthSession) {
        prefs.edit {
            putString(KEY_ACCESS, session.accessToken)
            putString(KEY_REFRESH, session.refreshToken)
            putLong(KEY_EXPIRES, session.expiresAtEpochMs)
            putString(KEY_CLIENT_ID, session.clientId)
        }
    }

    fun load(): StoredAuthSession? {
        val access = prefs.getString(KEY_ACCESS, null) ?: return null
        val refresh = prefs.getString(KEY_REFRESH, null)
        val expires = prefs.getLong(KEY_EXPIRES, 0L)
        val clientId = prefs.getString(KEY_CLIENT_ID, null)
        if (access.isBlank() || clientId.isNullOrBlank()) return null
        return StoredAuthSession(
            accessToken = access,
            refreshToken = refresh,
            expiresAtEpochMs = expires,
            clientId = clientId,
        )
    }

    fun clear() {
        prefs.edit { clear() }
    }

    fun savePendingOAuthVerifier(verifier: String) {
        prefs.edit { putString(KEY_OAUTH_VERIFIER, verifier) }
    }

    fun consumePendingOAuthVerifier(): String? {
        val verifier = prefs.getString(KEY_OAUTH_VERIFIER, null)
        if (verifier != null) {
            prefs.edit { remove(KEY_OAUTH_VERIFIER) }
        }
        return verifier
    }

    companion object {
        private const val PREFS_NAME = "arrow_puzzle_auth"
        private const val KEY_ACCESS = "access_token"
        private const val KEY_REFRESH = "refresh_token"
        private const val KEY_EXPIRES = "expires_at_ms"
        private const val KEY_CLIENT_ID = "client_id"
        private const val KEY_OAUTH_VERIFIER = "oauth_pkce_verifier"
    }
}
