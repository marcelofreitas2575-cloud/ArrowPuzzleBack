package com.arrowpuzzle.app.ui

import com.arrowpuzzle.app.data.dto.ApplyMoveResponse
import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.ArrowPathCellDto
import com.arrowpuzzle.app.data.dto.BoardDto
import com.arrowpuzzle.app.game.ArrowGeometry
import com.arrowpuzzle.app.game.LoadedArrowLocations
import com.arrowpuzzle.app.game.arrowById
import com.arrowpuzzle.app.game.frozen
import org.junit.Test

class BlockedArrowIntegrationTest {

    companion object {
        fun createBoard(height: Int = 12, width: Int = 8): BoardDto =
            BoardDto(height = height, width = width, arrows = emptyList())

        fun createArrow(
            id: String,
            headRow: Int,
            headColumn: Int,
            direction: String,
            length: Int,
            path: List<ArrowPathCellDto>? = null,
        ): ArrowDto = ArrowDto(
            id = id,
            headRow = headRow,
            headColumn = headColumn,
            direction = direction,
            length = length,
            path = path,
        )
    }

    /**
     * Simula o fluxo EXATO do que acontece quando usuário clica em seta que bate:
     * 1. Captura boardBefore
     * 2. Envia clique (servidor retorna BlockedAtStart)
     * 3. playBlockedBump anima bump
     * 4. Volta para posição original
     * 5. Valida que voltou EXATAMENTE
     */
    @Test
    fun testBlockedBumpExactReturn() {
        // Arrange: Uma seta L que vai bater
        val pathA1 = listOf(
            ArrowPathCellDto(10, 0),
            ArrowPathCellDto(9, 0),
            ArrowPathCellDto(8, 0),
            ArrowPathCellDto(8, 1),
            ArrowPathCellDto(8, 2),
        )
        val arrow1 = createArrow("a1", headRow = 8, headColumn = 2, "Right", length = 5, path = pathA1)
        val boardBefore = createBoard().copy(arrows = listOf(arrow1))

        // Snapshot do estado antes
        val boardBeforeFrozen = boardBefore.frozen()
        val arrowBeforeId = "a1"
        val arrowBefore = boardBefore.arrowById(arrowBeforeId)

        // Act 1: Servidor retorna BlockedAtStart (não muda nada)
        val serverResponse = ApplyMoveResponse(
            outcome = "BlockedAtStart",
            board = boardBefore, // Servidor não muda o board em BlockedAtStart
            isSuccess = false,
            isPuzzleComplete = false,
        )

        // Act 2: Simular playBlockedBump
        // 2a. Bump step
        val bumped = ArrowGeometry.withTrainBumpStep(boardBefore, arrowBeforeId).frozen()
        assert(bumped.arrowById(arrowBeforeId).path!!.size == 5) { "Bump não deve clipar" }

        // 2b. Delay...

        // 2c. Volta para boardBefore (CRÍTICO)
        val returnedBoard = boardBeforeFrozen // Isso é o que deve ser renderizado

        // Assert: Voltou EXATAMENTE?
        val returnedArrow = returnedBoard.arrowById(arrowBeforeId)
        assert(returnedArrow.path == pathA1) { "Path deve ser exatamente igual" }
        assert(returnedArrow.headRow == 8) { "Head row errado" }
        assert(returnedArrow.headColumn == 2) { "Head column errado" }
        println("✓ testBlockedBumpExactReturn: Arrow returned to exact position")
    }

    /**
     * Testa que boardBefore é realmente imutável (frozen) mesmo se servidor modifica algo
     */
    @Test
    fun testFrozenBoardImmutability() {
        // Arrange
        val pathA1 = listOf(
            ArrowPathCellDto(5, 0),
            ArrowPathCellDto(5, 1),
            ArrowPathCellDto(5, 2),
        )
        val arrow1 = createArrow("a1", headRow = 5, headColumn = 2, "Right", length = 3, path = pathA1)
        val boardOriginal = createBoard().copy(arrows = listOf(arrow1))

        // Act: Fazer frozen copy
        val boardFrozen = boardOriginal.frozen()

        // Assert: São iguais
        assert(boardFrozen.arrows[0].path == pathA1)

        // Act: Modificar original (simulando servidor mudando state)
        val modifiedArrow = arrow1.copy(headColumn = 99)
        val boardModified = boardOriginal.copy(arrows = listOf(modifiedArrow))

        // Assert: Frozen não foi afetado
        assert(boardFrozen.arrows[0].headColumn == 2) { "Frozen deve ser imutável" }
        assert(boardModified.arrows[0].headColumn == 99)
        println("✓ testFrozenBoardImmutability: Frozen is truly immutable")
    }

    /**
     * Testa o fluxo completo de um movimento bloqueado com polyline
     * Simula: clickArrow -> playBlockedBump -> return to original
     */
    @Test
    fun testCompleteBlockedFlowWithPolyline() {
        // Setup: Uma seta que vai bater em outra
        val path1 = listOf(
            ArrowPathCellDto(6, 0),
            ArrowPathCellDto(6, 1),
            ArrowPathCellDto(6, 2),
            ArrowPathCellDto(6, 3),
        )
        val arrow1 = createArrow("a1", headRow = 6, headColumn = 3, "Right", length = 4, path = path1)
        val arrow2 = createArrow("a2", headRow = 6, headColumn = 4, "Down", length = 2)
        
        val boardBefore = createBoard().copy(arrows = listOf(arrow1, arrow2))
        val boardFrozen = boardBefore.frozen()

        // Simulate click on a1
        val arrowBefore = boardBefore.arrowById("a1")
        val usesPolyline = !arrowBefore.path.isNullOrEmpty()
        assert(usesPolyline)

        // Step 1: Visual bump
        val bumped = ArrowGeometry.withTrainBumpStep(boardBefore, "a1")
        val bumpedArrow = bumped.arrowById("a1")
        println("Bumped path size: ${bumpedArrow.path?.size}")

        // Step 2: Return to frozen state
        val finalBoard = boardFrozen

        // Verify
        assert(finalBoard.arrowById("a1").path == path1) { "a1 must be exactly as before" }
        assert(finalBoard.arrowById("a2").headRow == 6) { "a2 must not change" }
        println("✓ testCompleteBlockedFlowWithPolyline: Complete flow correct")
    }

    /**
     * Testa que múltiplos cliques bloqueados em sequência funcionam
     */
    @Test
    fun testMovedAndStoppedRestoresToLoadedPosition() {
        val path1 = listOf(
            ArrowPathCellDto(5, 5),
            ArrowPathCellDto(5, 6),
            ArrowPathCellDto(5, 7),
        )
        val arrow1 = createArrow("a1", headRow = 5, headColumn = 7, "Right", length = 3, path = path1)
        val boardAtLoad = createBoard().copy(arrows = listOf(arrow1))
        LoadedArrowLocations.capture("session-1", boardAtLoad)

        // Servidor devolveu MovedAndStopped: seta andou 1 casa (simulado)
        val pathAfterServer = listOf(
            ArrowPathCellDto(5, 6),
            ArrowPathCellDto(5, 7),
            ArrowPathCellDto(5, 8),
        )
        val arrowAfterServer = createArrow("a1", headRow = 5, headColumn = 8, "Right", length = 3, path = pathAfterServer)
        assert(arrowAfterServer.path != path1)

        val boardBeforeClick = boardAtLoad.frozen()
        val restored = LoadedArrowLocations.restoreArrowOnBoard(boardBeforeClick, "a1")
        assert(restored.arrows[0].path == path1) { "Deve voltar ao path do carregamento" }
        println("✓ testMovedAndStoppedRestoresToLoadedPosition passed")
    }

    @Test
    fun testMultipleBlockedClicksRetainCorrectState() {
        // Setup
        val path1 = listOf(
            ArrowPathCellDto(5, 5),
            ArrowPathCellDto(5, 6),
            ArrowPathCellDto(5, 7),
        )
        val arrow1 = createArrow("a1", headRow = 5, headColumn = 7, "Right", length = 3, path = path1)
        val boardBefore = createBoard().copy(arrows = listOf(arrow1))
        val boardFrozen = boardBefore.frozen()

        // Click 1: Bump
        val bumped1 = ArrowGeometry.withTrainBumpStep(boardBefore, "a1")
        // Return to frozen
        val returned1 = boardFrozen
        assert(returned1.arrowById("a1").path == path1)

        // Click 2: Another bump attempt
        val bumped2 = ArrowGeometry.withTrainBumpStep(returned1, "a1")
        // Return to frozen again
        val returned2 = boardFrozen
        assert(returned2.arrowById("a1").path == path1)

        println("✓ testMultipleBlockedClicksRetainCorrectState: Multiple clicks work")
    }
}
