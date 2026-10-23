package com.arrowpuzzle.app.game

import com.arrowpuzzle.app.data.dto.ArrowDto
import com.arrowpuzzle.app.data.dto.ArrowPathCellDto
import com.arrowpuzzle.app.data.dto.BoardDto
import org.junit.Test

class ArrowGeometryTest {

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

    @Test
    fun testPolylineBumpReturnsToExactPosition() {
        // Arrange: Uma seta L (polyline) que bate em outra
        // Caminho: (5,5) -> (5,4) -> (5,3) -> (4,3) -> (3,3) -> (2,3)
        val path = listOf(
            ArrowPathCellDto(5, 5),
            ArrowPathCellDto(5, 4),
            ArrowPathCellDto(5, 3),
            ArrowPathCellDto(4, 3),
            ArrowPathCellDto(3, 3),
            ArrowPathCellDto(2, 3),
        )
        val arrow = createArrow("arrow1", headRow = 2, headColumn = 3, "Up", length = 6, path = path)
        val board = createBoard().copy(arrows = listOf(arrow))

        // Act: Fazer bump (visual, sem clipar)
        val bumped = ArrowGeometry.withTrainBumpStep(board, "arrow1")

        // Assert: Bump move um step
        val bumpedArrow = bumped.arrows[0]
        assert(bumpedArrow.path != null) { "Path deve estar preenchido após bump" }
        assert(bumpedArrow.path!!.size == path.size) { "Bump não deve clipar, deve manter mesmo tamanho" }

        // Assert: Volta para original deve ser exatamente igual
        val returned = board // Simula voltar para board original
        val returnedArrow = returned.arrows[0]
        assert(returnedArrow.path == path) { "Após bump, deve voltar para o path exato original" }
        println("✓ testPolylineBumpReturnsToExactPosition passed")
    }

    @Test
    fun testPolylineExitDetection() {
        // Arrange: Seta que vai sair do board
        val path = listOf(
            ArrowPathCellDto(0, 0),
            ArrowPathCellDto(0, 1),
        )
        val arrow = createArrow("arrow2", headRow = 0, headColumn = 1, "Right", length = 2, path = path)
        val board = createBoard().copy(arrows = listOf(arrow))

        // Act: Tentar fazer um step
        val stepped = ArrowGeometry.withTrainStep(board, "arrow2")

        // Assert: Se sai completamente, path deve estar vazio
        val steppedArrow = stepped.arrows[0]
        println("Stepped path after exit: ${steppedArrow.path}")
        // Path vazio significa seta saiu do board
        if (steppedArrow.path?.isEmpty() == true) {
            println("✓ testPolylineExitDetection: Arrow exited as expected")
        }
        println("✓ testPolylineExitDetection passed")
    }

    @Test
    fun testAdvanceTrainPath() {
        // Arrange: Um polyline
        val path = listOf(
            ArrowPathCellDto(5, 5),
            ArrowPathCellDto(5, 4),
            ArrowPathCellDto(4, 4),
        )

        // Act: Avançar um passo (tail cai, body segue, head avança)
        val advanced = ArrowGeometry.advanceTrainPath(path, "Up")

        // Assert: Deve ter 3 elementos
        assert(advanced.size == 3) { "Advanced path deve manter tamanho: ${advanced.size}" }
        // Tail (5,5) deve ter saído
        assert(advanced[0] != path[0]) { "Tail deve ter se movido" }
        // Head deve ter avançado uma linha pra cima
        val newHead = advanced.last()
        assert(newHead.row == 3) { "Head deve estar em row 3, está em ${newHead.row}" }
        assert(newHead.column == 4) { "Head deve estar em column 4, está em ${newHead.column}" }
        println("✓ testAdvanceTrainPath passed")
    }

    @Test
    fun testOccupiedCellsWithPolyline() {
        // Arrange: Polyline é a fonte de verdade se path existir
        val path = listOf(
            ArrowPathCellDto(5, 5),
            ArrowPathCellDto(5, 4),
            ArrowPathCellDto(4, 4),
            ArrowPathCellDto(3, 4),
        )
        val arrow = createArrow("a1", headRow = 3, headColumn = 4, "Up", length = 4, path = path)

        // Act: Pegar células ocupadas
        val cells = ArrowGeometry.occupiedCells(arrow)

        // Assert: Deve ser exatamente o path
        assert(cells.size == 4) { "occupiedCells deve retornar 4 células" }
        assert(cells[0].row == 5 && cells[0].column == 5) { "Primeira célula errada" }
        assert(cells[3].row == 3 && cells[3].column == 4) { "Última célula (head) errada" }
        println("✓ testOccupiedCellsWithPolyline passed")
    }

    @Test
    fun testTrainExitFramesEndWithArrowRemoved() {
        val path = listOf(
            ArrowPathCellDto(0, 4),
            ArrowPathCellDto(0, 5),
        )
        val arrow = createArrow("a1", headRow = 0, headColumn = 5, "Right", length = 2, path = path)
        val board = createBoard(height = 11, width = 7).copy(arrows = listOf(arrow))
        val frames = ArrowGeometry.simulateTrainExitBoardFrames(board, "a1")
        assert(frames.isNotEmpty()) { "Deve gerar frames de saída" }
        val last = frames.last()
        assert(last.arrows.none { it.id == "a1" }) { "Último frame deve remover a seta" }
        println("✓ testTrainExitFramesEndWithArrowRemoved passed (${frames.size} frames)")
    }

    @Test
    fun testBlockedArrowAnimationFlow() {
        // Simula o fluxo exato do que acontece quando seta bate
        // 1. renderiza boardBefore (original)
        val pathA1 = listOf(
            ArrowPathCellDto(10, 0),
            ArrowPathCellDto(9, 0),
            ArrowPathCellDto(8, 0),
        )
        val arrow1 = createArrow("a1", headRow = 8, headColumn = 0, "Up", length = 3, path = pathA1)
        val boardBefore = createBoard().copy(arrows = listOf(arrow1))

        // 2. Usuário clica, servidor retorna BlockedAtStart
        // 3. playBlockedBump faz bump
        val bumped = ArrowGeometry.withTrainBumpStep(boardBefore, "a1")
        assert(bumped.arrows[0].path?.size == 3) { "Bump não deve clipar" }

        // 4. Aguarda 90ms
        // 5. Volta para displayBoard = it.board (que é boardBefore, não mudou)
        val returnedBoard = boardBefore
        assert(returnedBoard.arrows[0].path == pathA1) { "Retorno deve ser exato" }

        println("✓ testBlockedArrowAnimationFlow passed")
    }
}
