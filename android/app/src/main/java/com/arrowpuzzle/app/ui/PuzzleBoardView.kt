package com.arrowpuzzle.app.ui

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.gestures.awaitEachGesture
import androidx.compose.foundation.gestures.awaitFirstDown
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.BoxWithConstraints
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.clipRect
import androidx.compose.ui.input.pointer.PointerEventPass
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.input.pointer.positionChange
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.platform.LocalViewConfiguration
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.BoardDto
import com.arrowpuzzle.app.settings.BackgroundStyle
import com.arrowpuzzle.app.settings.GameSettings
/** Tamanho fixo das células; tabuleiros grandes rolam na tela em vez de encolher. */
val DefaultCellSize = 28.dp

private val BoardFramePadding = 10.dp

@Composable
fun PuzzleBoardView(
    board: BoardDto,
    enabled: Boolean,
    onArrowClick: (ArrowDto) -> Unit,
    modifier: Modifier = Modifier,
    cellGap: Dp = 0.dp,
    cellSize: Dp = DefaultCellSize,
    showDirectionGuides: Boolean = false,
    arrowColorArgb: Int = GameSettings.DEFAULT_ARROW_COLOR,
    boardBackgroundStyle: BackgroundStyle = BackgroundStyle.GradientDecor,
) {
    val arrowInk = Color(arrowColorArgb)
    val density = LocalDensity.current
    val viewConfiguration = LocalViewConfiguration.current
    val boardTapKey = remember(board) {
        buildString {
            append(board.width).append('x').append(board.height).append('|')
            board.arrows.forEach { a ->
                append(a.id).append('@').append(a.headRow).append(',').append(a.headColumn).append(';')
            }
        }
    }

    BoxWithConstraints(
        modifier = modifier.fillMaxWidth(),
        contentAlignment = Alignment.Center,
    ) {
        val gap = cellGap
        val cols = board.width.coerceAtLeast(1)
        val rows = board.height.coerceAtLeast(1)
        val resolvedCellSize = cellSize

        val totalWidth = resolvedCellSize * cols + gap * (cols - 1).coerceAtLeast(0)
        val totalHeight = resolvedCellSize * rows + gap * (rows - 1).coerceAtLeast(0)
        val frameWidth = totalWidth + BoardFramePadding * 2
        val frameHeight = totalHeight + BoardFramePadding * 2

        Box(
            modifier = Modifier
                .width(frameWidth)
                .height(frameHeight)
                .clip(RoundedCornerShape(8.dp))
                .background(boardFrameTint(boardBackgroundStyle)),
        ) {
            Box(
                modifier = Modifier
                    .align(Alignment.Center)
                    .width(totalWidth)
                    .height(totalHeight)
                    .clip(RoundedCornerShape(4.dp))
                    .background(Color.White.copy(alpha = 0.92f)),
            ) {
                Canvas(
                    modifier = Modifier
                        .fillMaxSize()
                        .pointerInput(boardTapKey, enabled, resolvedCellSize, gap) {
                            if (!enabled) return@pointerInput
                            val cellPx = with(density) { resolvedCellSize.toPx() }
                            val gapPx = with(density) { gap.toPx() }
                            val step = cellPx + gapPx
                            val touchSlop = viewConfiguration.touchSlop

                            awaitEachGesture {
                                val down = awaitFirstDown(requireUnconsumed = false, pass = PointerEventPass.Initial)
                                val start = down.position
                                var travelled = 0f
                                while (true) {
                                    val event = awaitPointerEvent(pass = PointerEventPass.Initial)
                                    val change = event.changes.firstOrNull { it.id == down.id } ?: break
                                    if (!change.pressed) {
                                        if (travelled < touchSlop) {
                                            ArrowHitTesting.findArrowAt(start, board, step, cellPx)?.let(onArrowClick)
                                        }
                                        break
                                    }
                                    travelled += change.positionChange().getDistance()
                                    if (travelled >= touchSlop) break
                                }
                            }
                        },
                ) {
                    val cellPx = resolvedCellSize.toPx()
                    val gapPx = gap.toPx()
                    val step = cellPx + gapPx
                    val boardWidthPx = cols * step - gapPx
                    val boardHeightPx = rows * step - gapPx

                    clipRect(right = boardWidthPx, bottom = boardHeightPx) {
                        if (showDirectionGuides) {
                            with(ArrowPathDrawing) {
                                drawDirectionGuides(board, step, cellPx, boardWidthPx, boardHeightPx)
                            }
                        }
                        board.arrows.forEach { arrow ->
                            with(ArrowPathDrawing) {
                                drawArrowPath(arrow, board, step, cellPx, arrowInk)
                            }
                        }
                    }
                }
            }
        }
    }
}

private fun boardFrameTint(style: BackgroundStyle): Color =
    when (style) {
        BackgroundStyle.GradientDecor -> Color.White.copy(alpha = 0.35f)
        BackgroundStyle.Light -> Color(0xFFE8E8EC)
        BackgroundStyle.Dark -> Color(0xFF3A3A44)
        BackgroundStyle.Plain -> Color(0xFFECEFF1)
    }

