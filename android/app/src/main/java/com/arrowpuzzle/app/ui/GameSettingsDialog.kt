package com.arrowpuzzle.app.ui

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ExperimentalLayoutApi
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.arrowpuzzle.app.settings.BackgroundStyle
import com.arrowpuzzle.app.settings.GameSettings
import com.arrowpuzzle.app.settings.GameSettingsStore

@OptIn(ExperimentalLayoutApi::class)
@Composable
fun GameSettingsDialog(
    settings: GameSettings,
    onDismiss: () -> Unit,
    onSettingsChange: (GameSettings) -> Unit,
) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = {
            Text(text = "Configurações", fontWeight = FontWeight.Bold)
        },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {
                RowSetting(
                    label = "Ativar som",
                    trailing = {
                        Switch(
                            checked = settings.soundEnabled,
                            onCheckedChange = { checked ->
                                onSettingsChange(settings.copy(soundEnabled = checked))
                            },
                        )
                    },
                )

                Text(
                    text = "Cor da seta",
                    style = MaterialTheme.typography.titleSmall,
                    fontWeight = FontWeight.SemiBold,
                )
                FlowRow(
                    horizontalArrangement = Arrangement.spacedBy(12.dp),
                    verticalArrangement = Arrangement.spacedBy(10.dp),
                ) {
                    GameSettingsStore.ARROW_COLOR_OPTIONS.forEach { (label, argb) ->
                        val selected = settings.arrowColorArgb == argb
                        Column(horizontalAlignment = Alignment.CenterHorizontally) {
                            Box(contentAlignment = Alignment.Center) {
                                if (selected) {
                                    Box(
                                        modifier = Modifier
                                            .size(46.dp)
                                            .clip(CircleShape)
                                            .border(
                                                width = 3.dp,
                                                color = MaterialTheme.colorScheme.primary,
                                                shape = CircleShape,
                                            ),
                                    )
                                }
                                Box(
                                    modifier = Modifier
                                        .size(if (selected) 38.dp else 34.dp)
                                        .clip(CircleShape)
                                        .background(Color(argb))
                                        .border(
                                            width = if (selected) 2.dp else 1.dp,
                                            color = if (selected) {
                                                Color.White
                                            } else {
                                                Color.Gray.copy(alpha = 0.4f)
                                            },
                                            shape = CircleShape,
                                        )
                                        .clickable {
                                            onSettingsChange(settings.copy(arrowColorArgb = argb))
                                        },
                                )
                            }
                            Text(
                                text = label,
                                style = MaterialTheme.typography.labelSmall,
                                fontWeight = if (selected) FontWeight.Bold else FontWeight.Normal,
                                color = if (selected) {
                                    MaterialTheme.colorScheme.primary
                                } else {
                                    MaterialTheme.colorScheme.onSurface
                                },
                            )
                        }
                    }
                }

                Text(
                    text = "Fundo do app",
                    style = MaterialTheme.typography.titleSmall,
                    fontWeight = FontWeight.SemiBold,
                )
                FlowRow(
                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp),
                ) {
                    backgroundOption(
                        label = "Gradiente",
                        selected = settings.backgroundStyle == BackgroundStyle.GradientDecor,
                        onClick = {
                            onSettingsChange(
                                settings.copy(backgroundStyle = BackgroundStyle.GradientDecor),
                            )
                        },
                    )
                    backgroundOption(
                        label = "Claro",
                        selected = settings.backgroundStyle == BackgroundStyle.Light,
                        onClick = {
                            onSettingsChange(settings.copy(backgroundStyle = BackgroundStyle.Light))
                        },
                    )
                    backgroundOption(
                        label = "Escuro",
                        selected = settings.backgroundStyle == BackgroundStyle.Dark,
                        onClick = {
                            onSettingsChange(settings.copy(backgroundStyle = BackgroundStyle.Dark))
                        },
                    )
                    backgroundOption(
                        label = "Simples",
                        selected = settings.backgroundStyle == BackgroundStyle.Plain,
                        onClick = {
                            onSettingsChange(settings.copy(backgroundStyle = BackgroundStyle.Plain))
                        },
                    )
                }
            }
        },
        confirmButton = {
            TextButton(onClick = onDismiss) {
                Text("Fechar")
            }
        },
    )
}

@Composable
private fun RowSetting(
    label: String,
    trailing: @Composable () -> Unit,
) {
    Box(
        modifier = Modifier.fillMaxWidth(),
        contentAlignment = Alignment.CenterStart,
    ) {
        Text(
            text = label,
            style = MaterialTheme.typography.bodyLarge,
            modifier = Modifier.align(Alignment.CenterStart),
        )
        Box(modifier = Modifier.align(Alignment.CenterEnd)) {
            trailing()
        }
    }
}

@Composable
private fun backgroundOption(
    label: String,
    selected: Boolean,
    onClick: () -> Unit,
) {
    Text(
        text = label,
        modifier = Modifier
            .clip(RoundedCornerShape(20.dp))
            .background(
                if (selected) {
                    MaterialTheme.colorScheme.primary.copy(alpha = 0.15f)
                } else {
                    MaterialTheme.colorScheme.surfaceVariant
                },
            )
            .border(
                width = if (selected) 2.dp else 0.dp,
                color = MaterialTheme.colorScheme.primary,
                shape = RoundedCornerShape(20.dp),
            )
            .clickable(onClick = onClick)
            .padding(horizontal = 12.dp, vertical = 8.dp),
        style = MaterialTheme.typography.labelLarge,
        fontWeight = if (selected) FontWeight.Bold else FontWeight.Normal,
        color = MaterialTheme.colorScheme.onSurface,
    )
}
