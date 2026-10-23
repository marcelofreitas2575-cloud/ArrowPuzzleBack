package com.arrowpuzzle.app.game

import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.BoardDto

/** Cópia defensiva para animação (Compose ignora recomposição se a referência não mudar). */
fun BoardDto.frozen(): BoardDto =
    copy(
        arrows = arrows.map { arrow ->
            arrow.copy(path = arrow.path?.toList() ?: arrow.path)
        },
    )

fun BoardDto.arrowById(arrowId: String): ArrowDto =
    arrows.first { it.id == arrowId }
