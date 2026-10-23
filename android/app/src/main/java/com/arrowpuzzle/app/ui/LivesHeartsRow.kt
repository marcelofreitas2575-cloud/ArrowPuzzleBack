package com.arrowpuzzle.app.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.size
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.unit.dp
import com.arrowpuzzle.app.R
import androidx.compose.material3.Icon

@Composable
fun LivesHeartsRow(
    lives: Int,
    maxLives: Int = 3,
    modifier: Modifier = Modifier,
) {
    Row(
        modifier = modifier,
        horizontalArrangement = Arrangement.spacedBy(6.dp, Alignment.CenterHorizontally),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        repeat(maxLives) { index ->
            val filled = index < lives.coerceIn(0, maxLives)
            Icon(
                painter = painterResource(R.drawable.ic_heart_filled),
                contentDescription = null,
                modifier = Modifier.size(22.dp),
                tint = if (filled) {
                    androidx.compose.ui.graphics.Color(0xFFE53935)
                } else {
                    androidx.compose.ui.graphics.Color(0xFFE53935).copy(alpha = 0.22f)
                },
            )
        }
    }
}
