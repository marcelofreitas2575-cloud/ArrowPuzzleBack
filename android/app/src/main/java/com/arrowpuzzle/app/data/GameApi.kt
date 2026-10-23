package com.arrowpuzzle.app.data

import com.arrowpuzzle.app.data.dto.ApplyMoveRequest
import com.arrowpuzzle.app.data.dto.DevTokenRequest
import com.arrowpuzzle.app.data.dto.ProductInfoResponse
import com.arrowpuzzle.app.data.dto.TokenResponse
import com.arrowpuzzle.app.data.dto.ApplyMoveResponse
import com.arrowpuzzle.app.data.dto.SessionResponse
import com.arrowpuzzle.app.data.dto.StartSessionRequest
import com.arrowpuzzle.app.data.dto.CompleteLevelRequest
import com.arrowpuzzle.app.data.dto.PlayerProgressResponse
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.POST
import retrofit2.http.Path

interface GameApi {
    @GET("api/v1/product")
    suspend fun getProduct(): ProductInfoResponse

    @POST("api/v1/auth/dev-token")
    suspend fun requestDevToken(
        @Body body: DevTokenRequest = DevTokenRequest(),
    ): TokenResponse

    @GET("api/v1/me/progress")
    suspend fun getProgress(
        @Header("Authorization") authorization: String,
    ): PlayerProgressResponse

    @POST("api/v1/me/progress/complete")
    suspend fun completeLevel(
        @Header("Authorization") authorization: String,
        @Body body: CompleteLevelRequest,
    ): PlayerProgressResponse

    @POST("api/v1/sessions")
    suspend fun startSession(
        @Header("Authorization") authorization: String,
        @Body body: StartSessionRequest,
    ): SessionResponse

    @POST("api/v1/sessions/{sessionId}/moves")
    suspend fun applyMove(
        @Header("Authorization") authorization: String,
        @Path("sessionId") sessionId: String,
        @Body body: ApplyMoveRequest,
    ): ApplyMoveResponse
}
