package com.arrowpuzzle.app.data

import com.arrowpuzzle.app.data.dto.TokenResponse
import retrofit2.http.Field
import retrofit2.http.FormUrlEncoded
import retrofit2.http.POST

interface KeycloakApi {
    @FormUrlEncoded
    @POST("token")
    suspend fun requestTokenPassword(
        @Field("grant_type") grantType: String = "password",
        @Field("client_id") clientId: String,
        @Field("username") username: String,
        @Field("password") password: String,
    ): TokenResponse

    @FormUrlEncoded
    @POST("token")
    suspend fun requestTokenRefresh(
        @Field("grant_type") grantType: String = "refresh_token",
        @Field("client_id") clientId: String,
        @Field("refresh_token") refreshToken: String,
    ): TokenResponse

    @FormUrlEncoded
    @POST("token")
    suspend fun requestTokenAuthCode(
        @Field("grant_type") grantType: String = "authorization_code",
        @Field("client_id") clientId: String,
        @Field("code") code: String,
        @Field("redirect_uri") redirectUri: String,
        @Field("code_verifier") codeVerifier: String,
    ): TokenResponse
}
