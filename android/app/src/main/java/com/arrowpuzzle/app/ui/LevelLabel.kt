package com.arrowpuzzle.app.ui

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.sp

fun formatLevelNumber(levelId: String?): Int =
    levelId?.trim()?.toIntOrNull()?.coerceAtLeast(1) ?: 1

@Composable
fun LevelLabel(
    levelId: String?,
    modifier: Modifier = Modifier,
) {
    Text(
        text = "Nível ${formatLevelNumber(levelId)}",
        modifier = modifier,
        style = MaterialTheme.typography.titleLarge,
        fontWeight = FontWeight.Bold,
        fontSize = 22.sp,
        color = MaterialTheme.colorScheme.onBackground,
    )
}
