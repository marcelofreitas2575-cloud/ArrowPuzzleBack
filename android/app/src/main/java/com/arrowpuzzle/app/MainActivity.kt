package com.arrowpuzzle.app

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.ui.unit.dp
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Text
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.lifecycle.viewmodel.compose.viewModel
import com.arrowpuzzle.app.ui.AppScreenBackground
import com.arrowpuzzle.app.ui.AppViewModel
import com.arrowpuzzle.app.ui.GameScreen
import com.arrowpuzzle.app.ui.theme.ArrowPuzzleTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            ArrowPuzzleTheme {
                val viewModel: AppViewModel = viewModel()
                val uiState by viewModel.uiState.collectAsState()
                val gameSettings by viewModel.gameSettings.collectAsState()
                AppScreenBackground(backgroundStyle = gameSettings.backgroundStyle) {
                    when {
                        uiState.isBootstrapping || uiState.sessionId == null -> {
                            Box(
                                modifier = Modifier
                                    .fillMaxSize()
                                    .padding(24.dp),
                                contentAlignment = Alignment.Center,
                            ) {
                                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                    if (uiState.isLoading || uiState.isBootstrapping) {
                                        CircularProgressIndicator()
                                        Spacer(modifier = Modifier.height(16.dp))
                                    }
                                    uiState.errorMessage?.let { msg ->
                                        Text(text = msg)
                                    }
                                }
                            }
                        }
                        else -> {
                            GameScreen(
                                uiState = uiState,
                                gameSettings = gameSettings,
                                onSlideArrow = viewModel::slideArrow,
                                onRestartGame = viewModel::restartGameAfterGameOver,
                                onGameSettingsChange = viewModel::updateGameSettings,
                            )
                        }
                    }
                }
            }
        }
    }
}
