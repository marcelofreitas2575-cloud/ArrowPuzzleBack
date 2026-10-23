package com.arrowpuzzle.app.game

import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.ArrowPathCellDto
import com.arrowpuzzle.app.data.dto.BoardDto

/** Posição de uma seta no momento em que o tabuleiro foi carregado. */
data class ArrowLocationSnapshot(
    val id: String,
    val headRow: Int,
    val headColumn: Int,
    val direction: String,
    val length: Int,
    /** Células da cauda à cabeça (polyline); null se seta reta. */
    val path: List<ArrowPathCellDto>?,
) {
    companion object {
        fun from(arrow: ArrowDto): ArrowLocationSnapshot =
            ArrowLocationSnapshot(
                id = arrow.id,
                headRow = arrow.headRow,
                headColumn = arrow.headColumn,
                direction = arrow.direction,
                length = arrow.length,
                path = arrow.path?.map { ArrowPathCellDto(row = it.row, column = it.column) },
            )
    }
}

/**
 * Guarda em memória a localização de cada seta quando o board é carregado (início/reinício de sessão).
 */
object LoadedArrowLocations {
    private var loadedSessionId: String? = null
    private val locationsByArrowId: LinkedHashMap<String, ArrowLocationSnapshot> = linkedMapOf()

    fun capture(sessionId: String, board: BoardDto) {
        loadedSessionId = sessionId
        locationsByArrowId.clear()
        board.arrows.forEach { arrow ->
            locationsByArrowId[arrow.id] = ArrowLocationSnapshot.from(arrow)
        }
    }

    fun sessionId(): String? = loadedSessionId

    fun get(arrowId: String): ArrowLocationSnapshot? = locationsByArrowId[arrowId]

    fun all(): Map<String, ArrowLocationSnapshot> = locationsByArrowId.toMap()

    fun count(): Int = locationsByArrowId.size

    fun remove(arrowId: String) {
        locationsByArrowId.remove(arrowId)
    }

    /** Repõe uma seta no tabuleiro conforme a posição guardada no carregamento. */
    fun restoreArrowOnBoard(board: BoardDto, arrowId: String): BoardDto {
        val snapshot = get(arrowId) ?: return board
        if (board.arrows.none { it.id == arrowId }) {
            return board
        }
        return board.copy(
            arrows = board.arrows.map { arrow ->
                if (arrow.id == arrowId) snapshot.toArrowDto() else arrow
            },
        )
    }
}

fun ArrowLocationSnapshot.toArrowDto(): ArrowDto =
    ArrowDto(
        id = id,
        headRow = headRow,
        headColumn = headColumn,
        direction = direction,
        length = length,
        path = path?.map { ArrowPathCellDto(row = it.row, column = it.column) },
    )
