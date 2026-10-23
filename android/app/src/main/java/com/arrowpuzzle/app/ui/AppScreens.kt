package com.arrowpuzzle.app.ui

import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.ui.Alignment
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.arrowpuzzle.app.R
import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.settings.GameSettings

@Composable
fun GameScreen(
    uiState: AppUiState,
    gameSettings: GameSettings,
    onSlideArrow: (ArrowDto) -> Unit,
    onRestartGame: () -> Unit,
    onGameSettingsChange: (GameSettings) -> Unit,
) {
    var showDirectionGuides by remember { mutableStateOf(false) }
    var showSettingsDialog by remember { mutableStateOf(false) }
    val boardEnabled =
        !uiState.isAnimating && !uiState.isLoading && !uiState.showGameOverDialog

    if (uiState.showGameOverDialog) {
        AlertDialog(
            onDismissRequest = { },
            title = {
                Text(
                    text = "Sem vidas",
                    fontWeight = FontWeight.Bold,
                )
            },
            text = {
                Text("Você perdeu todas as vidas neste nível. Toque em reiniciar para tentar de novo.")
            },
            confirmButton = {
                Button(onClick = onRestartGame) {
                    Text("Reiniciar")
                }
            },
        )
    }

    if (showSettingsDialog) {
        GameSettingsDialog(
            settings = gameSettings,
            onDismiss = { showSettingsDialog = false },
            onSettingsChange = onGameSettingsChange,
        )
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
    ) {
        Text(
            text = uiState.gameTitle ?: stringResource(R.string.game_title_fallback),
            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.fillMaxWidth(),
            textAlign = TextAlign.Center,
        )
        val boardToShow = uiState.displayBoard ?: uiState.board

        Box(
            modifier = Modifier
                .fillMaxWidth()
                .padding(top = 6.dp),
        ) {
            LevelLabel(
                levelId = uiState.currentLevelId,
                modifier = Modifier.align(Alignment.Center),
            )
            SettingsGearButton(
                enabled = !uiState.isAnimating,
                onClick = { showSettingsDialog = true },
                modifier = Modifier.align(Alignment.CenterEnd),
            )
        }

        Box(
            modifier = Modifier
                .fillMaxWidth()
                .padding(top = 8.dp),
        ) {
            LivesHeartsRow(
                lives = uiState.lives,
                modifier = Modifier.align(Alignment.Center),
            )
            if (boardToShow != null) {
                ArrowRemainingBadge(
                    remainingCount = boardToShow.arrows.size,
                    modifier = Modifier.align(Alignment.CenterStart),
                )
            }
            GuidePathsToggleButton(
                active = showDirectionGuides,
                enabled = !uiState.isAnimating && !uiState.isLoading,
                compact = true,
                onClick = { showDirectionGuides = !showDirectionGuides },
                modifier = Modifier.align(Alignment.CenterEnd),
            )
        }
        uiState.errorMessage?.let {
            Text(
                it,
                color = MaterialTheme.colorScheme.error,
                modifier = Modifier.padding(top = 6.dp),
            )
        }

        boardToShow?.let { board ->
            val hScroll = rememberScrollState()
            val vScroll = rememberScrollState()
            Box(
                modifier = Modifier
                    .weight(1f)
                    .fillMaxWidth()
                    .padding(top = 10.dp),
                contentAlignment = Alignment.TopCenter,
            ) {
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .verticalScroll(vScroll)
                        .horizontalScroll(hScroll),
                    contentAlignment = Alignment.TopCenter,
                ) {
                    PuzzleBoardView(
                        board = board,
                        enabled = boardEnabled,
                        onArrowClick = onSlideArrow,
                        showDirectionGuides = showDirectionGuides,
                        arrowColorArgb = gameSettings.arrowColorArgb,
                        boardBackgroundStyle = gameSettings.backgroundStyle,
                    )
                }
            }
        }

        if (uiState.isLoading && !uiState.isAnimating) {
            CircularProgressIndicator(modifier = Modifier.padding(top = 12.dp))
        }
    }
}
