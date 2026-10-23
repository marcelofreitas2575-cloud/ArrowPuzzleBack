package com.arrowpuzzle.app.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import com.arrowpuzzle.app.settings.BackgroundStyle

@Composable
fun AppScreenBackground(
    backgroundStyle: BackgroundStyle,
    modifier: Modifier = Modifier,
    content: @Composable () -> Unit,
) {
    Box(modifier = modifier.fillMaxSize()) {
        when (backgroundStyle) {
            BackgroundStyle.GradientDecor -> {
                Box(
                    Modifier
                        .fillMaxSize()
                        .background(
                            Brush.linearGradient(
                                colors = listOf(
                                    Color(0xFFEDE7F6),
                                    Color(0xFFE1BEE7),
                                ),
                            ),
                        ),
                )
            }
            BackgroundStyle.Light -> {
                Box(Modifier.fillMaxSize().background(Color(0xFFFAFAFA)))
            }
            BackgroundStyle.Dark -> {
                Box(Modifier.fillMaxSize().background(Color(0xFF121218)))
            }
            BackgroundStyle.Plain -> {
                Box(Modifier.fillMaxSize().background(Color(0xFFECEFF1)))
            }
        }
        content()
    }
}
