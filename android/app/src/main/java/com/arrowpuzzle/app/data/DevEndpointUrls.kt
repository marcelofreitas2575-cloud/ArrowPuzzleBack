package com.arrowpuzzle.app.data

import com.arrowpuzzle.app.BuildConfig
import okhttp3.HttpUrl.Companion.toHttpUrlOrNull

internal object DevEndpointUrls {
    fun bffBaseUrl(): String = normalizeHttpBase(BuildConfig.BFF_BASE_URL)

    fun keycloakBaseUrl(): String = normalizeHttpBase(BuildConfig.KEYCLOAK_BASE_URL)

    private fun normalizeHttpBase(raw: String): String {
        var value = raw.trim().trimEnd('/')
        if (value.isEmpty()) {
            throw IllegalStateException("URL de backend vazia no BuildConfig.")
        }
        if (value.startsWith("//")) {
            value = "http:$value"
        }
        if (!value.startsWith("http://", ignoreCase = true) &&
            !value.startsWith("https://", ignoreCase = true)
        ) {
            value = "http://$value"
        }
        value.toHttpUrlOrNull()
            ?: throw IllegalStateException("URL de backend inválida: $raw")
        return if (value.endsWith("/")) value else "$value/"
    }
}
