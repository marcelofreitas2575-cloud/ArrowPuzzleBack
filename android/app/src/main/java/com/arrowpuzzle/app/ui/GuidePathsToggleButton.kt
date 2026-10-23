package com.arrowpuzzle.app.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.interaction.MutableInteractionSource
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Icon
import androidx.compose.material3.ripple
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.semantics.Role
import androidx.compose.ui.unit.dp
import com.arrowpuzzle.app.R

@Composable
fun GuidePathsToggleButton(
    active: Boolean,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true,
    compact: Boolean = false,
) {
    val guideBlue = Color(0xFF2196F3)
    val buttonSize = if (compact) 40.dp else 52.dp
    val iconSize = if (compact) 22.dp else 26.dp
    Box(
        modifier = modifier
            .shadow(elevation = if (compact) 4.dp else 6.dp, shape = CircleShape, clip = false)
            .size(buttonSize)
            .clip(CircleShape)
            .background(Color.White)
            .clickable(
                enabled = enabled,
                role = Role.Button,
                interactionSource = remember { MutableInteractionSource() },
                indication = ripple(bounded = true),
                onClick = onClick,
            ),
        contentAlignment = Alignment.Center,
    ) {
        Icon(
            painter = painterResource(R.drawable.ic_guide_paths),
            contentDescription = if (active) "Ocultar linhas de direção" else "Mostrar linhas de direção",
            modifier = Modifier.size(iconSize),
            tint = if (active) guideBlue else guideBlue.copy(alpha = 0.72f),
        )
    }
}
