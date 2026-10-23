package com.arrowpuzzle.app.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

private val Ink = Color(0xFF1F1635)
private val Plum = Color(0xFF5E4B8B)
private val Lavender = Color(0xFFEDE7F6)
private val SoftSurface = Color(0xFFF8F6FC)

private val LightColors = lightColorScheme(
    primary = Plum,
    onPrimary = Color.White,
    secondary = Ink,
    onSecondary = Color.White,
    background = SoftSurface,
    onBackground = Ink,
    surface = Color.White,
    onSurface = Ink,
    surfaceVariant = Lavender,
    onSurfaceVariant = Ink.copy(alpha = 0.72f),
    outline = Plum.copy(alpha = 0.35f),
)

@Composable
fun ArrowPuzzleTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = LightColors,
        content = content,
    )
}
