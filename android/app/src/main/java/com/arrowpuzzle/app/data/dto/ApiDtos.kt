package com.arrowpuzzle.app.data.dto

import com.squareup.moshi.Json

data class StartSessionRequest(
    @Json(name = "levelId") val levelId: String,
)

data class SessionResponse(
    @Json(name = "sessionId") val sessionId: String,
    @Json(name = "board") val board: BoardDto,
)

data class BoardDto(
    @Json(name = "width") val width: Int,
    @Json(name = "height") val height: Int,
    @Json(name = "arrows") val arrows: List<ArrowDto>,
)

data class ArrowPathCellDto(
    @Json(name = "row") val row: Int,
    @Json(name = "column") val column: Int,
)

data class ArrowDto(
    @Json(name = "id") val id: String,
    @Json(name = "headRow") val headRow: Int,
    @Json(name = "headColumn") val headColumn: Int,
    @Json(name = "direction") val direction: String,
    @Json(name = "length") val length: Int,
    @Json(name = "path") val path: List<ArrowPathCellDto>? = null,
)

data class ApplyMoveRequest(
    @Json(name = "arrowId") val arrowId: String,
)

data class ApplyMoveResponse(
    @Json(name = "outcome") val outcome: String,
    @Json(name = "board") val board: BoardDto,
    @Json(name = "isSuccess") val isSuccess: Boolean,
    @Json(name = "isPuzzleComplete") val isPuzzleComplete: Boolean,
)

data class ProductInfoResponse(
    @Json(name = "displayName") val displayName: String,
)

data class DevTokenRequest(
    @Json(name = "username") val username: String? = null,
    @Json(name = "password") val password: String? = null,
)

data class TokenResponse(
    @Json(name = "access_token") val accessToken: String,
    @Json(name = "refresh_token") val refreshToken: String? = null,
    @Json(name = "expires_in") val expiresIn: Long? = null,
)
