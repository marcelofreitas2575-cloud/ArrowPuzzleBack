# Geração procedural de níveis (offline) — notas de reversão

**Data:** 2026-03-22  
**Status:** algoritmo + testes only — **não integrado** ao Level Catalog, JSON `001.json`, Android ou Docker.

## O que foi adicionado

| Caminho | Descrição |
|---------|-----------|
| `services/LevelGeneration/ArrowPuzzle.LevelGeneration/` | Biblioteca: solver, gerador reverso, parâmetros por nível |
| `services/LevelGeneration/ArrowPuzzle.LevelGeneration.Tests/` | Testes xUnit (muitos seeds, duas estratégias 3.4) |
| `docs/LEVEL_GENERATION_REVERT.md` | Este arquivo |

## Guia para humanos

Leia **[LEVEL_GENERATION_GUIA.md](LEVEL_GENERATION_GUIA.md)** — o que fazer agora (nada) vs. o que falta para ir ao app.

## Estratégia 3.4 (decisão: Backtrack)

Enum `TailExtensionSafetyStrategy`:

1. **`BacktrackOnFailedExitTest`** (padrão) — estende cauda; se `Slide` ≠ `ExitedBoard`, desfaz e tenta outra célula (estilo Snek!).
2. **`ReserveExitCorridorCells`** — reserva células do corredor de saída; caudas novas não ocupam essas células (mais conservador, tabuleiro mais vazio).

Ambas implementadas; testes exigem sucesso mínimo em cada uma.

## Como reverter completamente

1. Remover pasta `services/LevelGeneration/`.
2. Remover entradas do projeto na `ArrowPuzzle.sln` (se adicionadas via `dotnet sln add`).
3. Apagar este doc (opcional).

Nenhum serviço em execução referencia o gerador ainda.

## Testes (2026-03-22)

```powershell
dotnet test services/LevelGeneration/ArrowPuzzle.LevelGeneration.Tests/ArrowPuzzle.LevelGeneration.Tests.csproj
```

**14 testes** — inclui 25 seeds × 4 faixas de nível, ambas estratégias 3.4, solver independente, nível 001 manual solvável.

## Recomendação 3.4 (para decidir depois)

| Estratégia | Prós | Contras |
|------------|------|---------|
| **BacktrackOnFailedExitTest** (default no código) | Tabuleiro mais cheio; mais sucessos de geração nos testes | Pode exigir mais tentativas por seta |
| **ReserveExitCorridorCells** | Menos risco de corredor “fechado” no futuro | Mais vazio; menos layouts aceitos |

Sugestão: **default Backtrack** no gerador; usar **Reserve** só em níveis altos se quiser puzzles mais “limpos”.

## Próximo passo (não feito — ver GUIA)

- CLI feita: `ArrowPuzzle.LevelGeneration.Cli` (export JSON)
- Falta: JSON no catálogo + app listar níveis + rebuild Docker catalog

## Memória do agente

Cópia resumida em: agent store `level-generation-revert-2026-03-22.md`.
