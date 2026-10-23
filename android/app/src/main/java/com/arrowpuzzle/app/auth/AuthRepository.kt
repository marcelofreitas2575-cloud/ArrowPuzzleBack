package com.arrowpuzzle.app.auth

import android.content.Intent
import android.net.Uri
import com.arrowpuzzle.app.BuildConfig
import com.arrowpuzzle.app.data.ServiceLocator
import com.arrowpuzzle.app.data.dto.TokenResponse
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock

class AuthRepository(
    private val sessionStore: AuthSessionStore,
) {
    private val refreshMutex = Mutex()

    val redirectUri: String = "com.arrowpuzzle.app://callback"

    suspend fun loginWithPassword(username: String, password: String): Result<String> =
        requestDevToken(username, password)

    suspend fun loginAsGuest(): Result<String> {
        val primary = requestDevToken(GUEST_USERNAME, GUEST_PASSWORD)
        if (primary.isSuccess) return primary
        return requestDevToken(DEV_FALLBACK_USERNAME, DEV_FALLBACK_PASSWORD)
    }

    private suspend fun requestDevToken(username: String, password: String): Result<String> =
        runCatching {
            val token = ServiceLocator.gameApi.requestDevToken(
                com.arrowpuzzle.app.data.dto.DevTokenRequest(
                    username = username,
                    password = password,
                ),
            )
            persistTokenResponse(token, clientId = BuildConfig.OIDC_DEV_CLIENT_ID)
            bearerFrom(token.accessToken)
        }.recoverCatching { ex ->
            val hint = when {
                ex.message?.contains("404", ignoreCase = true) == true ->
                    "dev-token desligado no gateway (KeycloakDevAuth__Enabled). Rode: docker compose up -d keycloak gateway"
                else ->
                    "Confira ${BuildConfig.BFF_BASE_URL}/health no Chrome. APK ${BuildConfig.VERSION_NAME}."
            }
            throw IllegalStateException("Falha no login convidado: $hint", ex)
        }

    suspend fun restoreBearerToken(): Result<String> {
        val stored = sessionStore.load() ?: return Result.failure(NoStoredSessionException())
        if (stored.refreshToken.isNullOrBlank()) {
            if (stored.expiresAtEpochMs > System.currentTimeMillis() + TOKEN_SKEW_MS) {
                return Result.success(bearerFrom(stored.accessToken))
            }
            sessionStore.clear()
            return Result.failure(SessionExpiredException())
        }
        if (stored.expiresAtEpochMs > System.currentTimeMillis() + TOKEN_SKEW_MS) {
            return Result.success(bearerFrom(stored.accessToken))
        }
        return refreshMutex.withLock {
            val latest = sessionStore.load() ?: return@withLock Result.failure(NoStoredSessionException())
            if (latest.expiresAtEpochMs > System.currentTimeMillis() + TOKEN_SKEW_MS) {
                return@withLock Result.success(bearerFrom(latest.accessToken))
            }
            runCatching {
                val refreshed = ServiceLocator.keycloakApi.requestTokenRefresh(
                    clientId = latest.clientId,
                    refreshToken = latest.refreshToken!!,
                )
                persistTokenResponse(
                    refreshed,
                    clientId = latest.clientId,
                    fallbackRefresh = latest.refreshToken,
                )
                bearerFrom(refreshed.accessToken)
            }.onFailure {
                sessionStore.clear()
            }
        }
    }

    fun clearSession() {
        sessionStore.clear()
    }

    fun buildGoogleSignInUri(): Uri {
        val verifier = PkceSupport.newCodeVerifier()
        sessionStore.savePendingOAuthVerifier(verifier)
        val challenge = PkceSupport.codeChallengeS256(verifier)
        return Uri.parse(
            "${oidcBase()}auth?" +
                "client_id=${Uri.encode(BuildConfig.OIDC_ANDROID_CLIENT_ID)}" +
                "&redirect_uri=${Uri.encode(redirectUri)}" +
                "&response_type=code" +
                "&scope=${Uri.encode("openid profile email")}" +
                "&code_challenge=${Uri.encode(challenge)}" +
                "&code_challenge_method=S256" +
                "&kc_idp_hint=google",
        )
    }

    fun buildCustomTabIntent(uri: Uri): Intent =
        androidx.browser.customtabs.CustomTabsIntent.Builder()
            .setShowTitle(true)
            .build()
            .intent
            .apply {
                data = uri
            }

    suspend fun completeOAuthRedirect(data: Uri): Result<String> {
        val error = data.getQueryParameter("error")
        if (!error.isNullOrBlank()) {
            val description = data.getQueryParameter("error_description")
            return Result.failure(IllegalStateException(description ?: error))
        }
        val code = data.getQueryParameter("code")
            ?: return Result.failure(IllegalStateException("Resposta de login incompleta."))
        val verifier = sessionStore.consumePendingOAuthVerifier()
            ?: return Result.failure(IllegalStateException("Sessão de login expirada. Tente de novo."))

        return runCatching {
            val token = ServiceLocator.keycloakApi.requestTokenAuthCode(
                clientId = BuildConfig.OIDC_ANDROID_CLIENT_ID,
                code = code,
                redirectUri = redirectUri,
                codeVerifier = verifier,
            )
            persistTokenResponse(token, clientId = BuildConfig.OIDC_ANDROID_CLIENT_ID)
            bearerFrom(token.accessToken)
        }
    }

    private fun persistTokenResponse(
        token: TokenResponse,
        clientId: String,
        fallbackRefresh: String? = null,
    ) {
        val refresh = token.refreshToken ?: fallbackRefresh
        val expiresInSec = token.expiresIn ?: DEFAULT_EXPIRES_SEC
        sessionStore.save(
            StoredAuthSession(
                accessToken = token.accessToken,
                refreshToken = refresh,
                expiresAtEpochMs = System.currentTimeMillis() + expiresInSec * 1000L,
                clientId = clientId,
            ),
        )
    }

    private fun bearerFrom(accessToken: String): String = "Bearer $accessToken"

    private fun oidcBase(): String =
        "${BuildConfig.KEYCLOAK_BASE_URL}/realms/${BuildConfig.OIDC_REALM}/protocol/openid-connect/"

    class NoStoredSessionException : Exception()
    class SessionExpiredException : Exception()

    companion object {
        private const val TOKEN_SKEW_MS = 60_000L
        private const val DEFAULT_EXPIRES_SEC = 300L
        private const val GUEST_USERNAME = "guest-player"
        private const val GUEST_PASSWORD = "guest-player"
        private const val DEV_FALLBACK_USERNAME = "dev-player"
        private const val DEV_FALLBACK_PASSWORD = "dev-player"
    }
}
