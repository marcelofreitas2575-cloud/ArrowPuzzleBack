package com.arrowpuzzle.app.data.dto

import com.squareup.moshi.Json
import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class PlayerProgressResponse(
    @Json(name = "userId") val userId: String,
    @Json(name = "completedLevelIds") val completedLevelIds: List<String> = emptyList(),
)

@JsonClass(generateAdapter = true)
data class CompleteLevelRequest(
    @Json(name = "levelId") val levelId: String,
)
