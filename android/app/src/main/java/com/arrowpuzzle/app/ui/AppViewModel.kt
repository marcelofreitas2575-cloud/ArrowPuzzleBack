package com.arrowpuzzle.app.ui

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.viewModelScope
import com.arrowpuzzle.app.R
import com.arrowpuzzle.app.auth.AuthRepository
import com.arrowpuzzle.app.auth.AuthSessionStore
import com.arrowpuzzle.app.data.ServiceLocator
import com.arrowpuzzle.app.data.dto.ApplyMoveRequest
import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.BoardDto
import com.arrowpuzzle.app.data.dto.StartSessionRequest
import com.arrowpuzzle.app.game.ArrowGeometry
import com.arrowpuzzle.app.game.LoadedArrowLocations
import com.arrowpuzzle.app.game.arrowById
import com.arrowpuzzle.app.game.frozen
import com.arrowpuzzle.app.progress.LocalProgressStore
import com.arrowpuzzle.app.progress.PlayerProgressRepository
import com.arrowpuzzle.app.settings.GameSettings
import com.arrowpuzzle.app.settings.GameSettingsStore
import com.arrowpuzzle.app.settings.GameSoundPlayer
import kotlinx.coroutines.CoroutineExceptionHandler
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

data class AppUiState(
    val isBootstrapping: Boolean = true,
    val isLoading: Boolean = false,
    val gameTitle: String? = null,
    val errorMessage: String? = null,
    val currentLevelId: String? = null,
    val sessionId: String? = null,
    val board: BoardDto? = null,
    val displayBoard: BoardDto? = null,
    val isAnimating: Boolean = false,
    val lastOutcome: String? = null,
    val puzzleComplete: Boolean = false,
    val lives: Int = 3,
    val showGameOverDialog: Boolean = false,
)

class AppViewModel(application: Application) : AndroidViewModel(application) {
    private val _uiState = MutableStateFlow(AppUiState())
    val uiState: StateFlow<AppUiState> = _uiState.asStateFlow()

    private val authRepository = AuthRepository(AuthSessionStore(application))
    private val localProgressStore = LocalProgressStore(application)
    private val playerProgressRepository = PlayerProgressRepository(localProgressStore)
    private val gameSettingsStore = GameSettingsStore(application)
    private val soundPlayer = GameSoundPlayer(application)
    private var bearerToken: String? = null

    private val _gameSettings = MutableStateFlow(gameSettingsStore.load())
    val gameSettings: StateFlow<GameSettings> = _gameSettings.asStateFlow()

    init {
        val bootstrapHandler = CoroutineExceptionHandler { _, throwable ->
            _uiState.update {
                it.copy(
                    isBootstrapping = false,
                    isLoading = false,
                    errorMessage = throwable.message ?: "Erro ao iniciar o jogo.",
                )
            }
        }
        viewModelScope.launch(bootstrapHandler) {
            val fallbackTitle = getApplication<Application>().getString(R.string.game_title_fallback)
            val productTitle = runCatching {
                ServiceLocator.gameApi.getProduct().displayName.trim()
            }.getOrNull()?.takeIf { it.isNotEmpty() }
            _uiState.update { it.copy(gameTitle = productTitle ?: fallbackTitle) }

            authRepository.clearSession()
            authRepository.loginAsGuest()
                .onSuccess { bearer ->
                    bearerToken = bearer
                    runCatching {
                        playerProgressRepository.fetchPlayableLevelId(bearer)
                    }.onSuccess { levelId ->
                        _uiState.update { it.copy(isBootstrapping = false, errorMessage = null) }
                        startLevelInternal(levelId)
                    }.onFailure { ex ->
                        _uiState.update {
                            it.copy(
                                isBootstrapping = false,
                                errorMessage = ex.message ?: "Falha ao carregar progresso no servidor.",
                            )
                        }
                    }
                }
                .onFailure { ex ->
                    _uiState.update {
                        it.copy(
                            isBootstrapping = false,
                            errorMessage = ex.message ?: "Não foi possível conectar ao jogo.",
                        )
                    }
                }
        }
    }

    fun updateGameSettings(settings: GameSettings) {
        gameSettingsStore.save(settings)
        _gameSettings.value = settings
    }

    override fun onCleared() {
        soundPlayer.release()
        super.onCleared()
    }

    private suspend fun startLevelInternal(levelId: String) {
        val token = bearerToken ?: return
        _uiState.update {
            it.copy(
                isLoading = true,
                errorMessage = null,
                showGameOverDialog = false,
                puzzleComplete = false,
                currentLevelId = levelId,
                lives = 3,
            )
        }
        runCatching {
            val session = ServiceLocator.gameApi.startSession(
                authorization = token,
                body = StartSessionRequest(levelId = levelId),
            )
            val board = session.board.frozen()
            LoadedArrowLocations.capture(session.sessionId, board)
            _uiState.update {
                it.copy(
                    isLoading = false,
                    sessionId = session.sessionId,
                    board = board,
                    displayBoard = null,
                    isAnimating = false,
                    lastOutcome = null,
                )
            }
        }.onFailure { ex ->
            _uiState.update {
                it.copy(
                    isLoading = false,
                    errorMessage = ex.message ?: "Falha ao iniciar sessão.",
                )
            }
        }
    }

    fun slideArrow(arrow: ArrowDto) {
        val token = bearerToken ?: return
        val sessionId = _uiState.value.sessionId ?: return
        if (_uiState.value.isAnimating || _uiState.value.isLoading || _uiState.value.showGameOverDialog) {
            return
        }

        viewModelScope.launch {
            val boardBefore = (_uiState.value.board ?: return@launch).frozen()
            _uiState.update { it.copy(errorMessage = null, isAnimating = true, displayBoard = null) }

            try {
            val result = runCatching {
                ServiceLocator.gameApi.applyMove(
                    authorization = token,
                    sessionId = sessionId,
                    body = ApplyMoveRequest(arrowId = arrow.id),
                )
            }

            result.onFailure { ex ->
                _uiState.update {
                    it.copy(
                        isAnimating = false,
                        displayBoard = null,
                        errorMessage = ex.message ?: "Falha ao aplicar movimento.",
                    )
                }
                return@launch
            }

            val response = result.getOrThrow()
            val arrowBefore = boardBefore.arrowById(arrow.id)
            val arrowAfter = response.board.arrows.firstOrNull { it.id == arrow.id }
            val usesPolyline = !arrowBefore.path.isNullOrEmpty()

            when (response.outcome) {
                "BlockedAtStart", "MovedAndStopped" -> {
                    playCollisionAnimation(
                        boardBefore = boardBefore,
                        arrowId = arrow.id,
                        arrowBefore = arrowBefore,
                        outcome = response.outcome,
                        usesPolyline = usesPolyline,
                    )
                    val restoredBoard =
                        LoadedArrowLocations.restoreArrowOnBoard(boardBefore, arrow.id).frozen()
                    _uiState.update {
                        it.copy(
                            board = restoredBoard,
                            displayBoard = null,
                            isAnimating = false,
                            lastOutcome = null,
                            puzzleComplete = false,
                        )
                    }
                    playCollisionFeedback()
                    applyLifeLossAfterCollision()
                }
                else -> {
                    when {
                        arrowAfter == null && usesPolyline -> {
                            playBoardFrames(
                                ArrowGeometry.simulateTrainExitBoardFrames(boardBefore, arrow.id),
                            )
                        }
                        arrowAfter == null -> {
                            playBoardFrames(
                                ArrowGeometry.simulateStraightExitBoardFrames(boardBefore, arrow.id),
                            )
                        }
                        usesPolyline -> {
                            playBoardFrames(
                                ArrowGeometry.replayTrainBoardFrames(
                                    boardBefore,
                                    arrow.id,
                                    arrowBefore,
                                    arrowAfter,
                                ),
                            )
                        }
                        else -> {
                            val (dr, dc) = ArrowGeometry.directionDelta(arrowBefore.direction)
                            val steps = maxOf(
                                kotlin.math.abs(arrowAfter.headRow - arrowBefore.headRow),
                                kotlin.math.abs(arrowAfter.headColumn - arrowBefore.headColumn),
                            )
                            if (steps > 0) {
                                var row = arrowBefore.headRow
                                var col = arrowBefore.headColumn
                                val frames = buildList {
                                    repeat(steps) {
                                        row += dr
                                        col += dc
                                        add(ArrowGeometry.withHead(boardBefore, arrow.id, row, col).frozen())
                                    }
                                }
                                playBoardFrames(frames)
                            }
                        }
                    }
                    if (response.outcome == "ExitedBoard") {
                        LoadedArrowLocations.remove(arrow.id)
                    }
                    _uiState.update {
                        it.copy(
                            board = response.board.frozen(),
                            displayBoard = null,
                            isAnimating = false,
                            lastOutcome = response.outcome,
                            puzzleComplete = response.isPuzzleComplete,
                        )
                    }
                    if (response.isPuzzleComplete) {
                        playWinFeedback()
                        onPuzzleWon()
                    }
                }
            }
            } catch (ex: Exception) {
                _uiState.update {
                    it.copy(
                        isAnimating = false,
                        displayBoard = null,
                        errorMessage = ex.message ?: "Falha na animação.",
                    )
                }
            }
        }
    }

    private suspend fun playBoardFrames(frames: List<BoardDto>, stepDelayMs: Long = 52) {
        for (frame in frames) {
            _uiState.update { state -> state.copy(displayBoard = frame.frozen()) }
            delay(stepDelayMs)
        }
    }

    private suspend fun onPuzzleWon() {
        val token = bearerToken ?: return
        val levelId = _uiState.value.currentLevelId ?: return
        val sessionId = _uiState.value.sessionId ?: return
        _uiState.update { it.copy(isLoading = true, errorMessage = null) }
        delay(400)
        runCatching {
            playerProgressRepository.completeLevelAndNextPlayable(token, levelId, sessionId)
        }.onSuccess { nextLevel ->
            startLevelInternal(nextLevel)
        }.onFailure { ex ->
            _uiState.update {
                it.copy(
                    isLoading = false,
                    errorMessage = ex.message ?: "Falha ao salvar progresso no servidor.",
                )
            }
        }
    }

    private suspend fun playCollisionAnimation(
        boardBefore: BoardDto,
        arrowId: String,
        arrowBefore: ArrowDto,
        outcome: String,
        usesPolyline: Boolean,
    ) {
        if (outcome == "MovedAndStopped") {
            val slideFrames = if (usesPolyline) {
                ArrowGeometry.simulateTrainSlideUntilBlockedFrames(boardBefore, arrowId)
            } else {
                ArrowGeometry.simulateStraightSlideUntilBlockedFrames(boardBefore, arrowId)
            }
            playBoardFrames(slideFrames, stepDelayMs = 38)
        }

        val bumpBase = boardBefore.frozen()
        val bumped = if (usesPolyline) {
            ArrowGeometry.withTrainBumpStep(bumpBase, arrowId).frozen()
        } else {
            val (dr, dc) = ArrowGeometry.directionDelta(arrowBefore.direction)
            val current = bumpBase.arrowById(arrowId)
            ArrowGeometry.withHead(
                bumpBase,
                arrowId,
                current.headRow + dr,
                current.headColumn + dc,
            ).frozen()
        }
        _uiState.update { it.copy(displayBoard = bumped) }
        delay(38)
        _uiState.update { it.copy(displayBoard = null) }
    }

    private suspend fun applyLifeLossAfterCollision() {
        val newLives = _uiState.value.lives - 1
        if (newLives <= 0) {
            _uiState.update {
                it.copy(
                    lives = 0,
                    showGameOverDialog = true,
                )
            }
        } else {
            _uiState.update { it.copy(lives = newLives) }
        }
    }

    fun restartGameAfterGameOver() {
        val levelId = _uiState.value.currentLevelId ?: return
        viewModelScope.launch {
            startLevelInternal(levelId)
        }
    }

    private fun playCollisionFeedback() {
        if (_gameSettings.value.soundEnabled) {
            soundPlayer.playCollision()
        }
    }

    private fun playWinFeedback() {
        if (_gameSettings.value.soundEnabled) {
            soundPlayer.playWin()
        }
    }
}
