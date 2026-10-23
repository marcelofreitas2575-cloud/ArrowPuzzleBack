# Geração de níveis — o que é, o que falta, o que você faz

## Resumo em uma frase

O **jogo que você usa hoje** (nível `001.json`, app, Docker) **não mudou**. Só existe um **motor de geração offline** testado; **não entra no app sozinho**.

---

## O que já está pronto

| Item | Onde |
|------|------|
| Gerador (construção reversa) | `services/LevelGeneration/ArrowPuzzle.LevelGeneration/` |
| Solver (confere se dá para vencer) | `PuzzleSolver` no mesmo projeto |
| Estratégia **3.4 = Backtrack** (decisão fixada) | `LevelGenerationDefaults.DefaultTailSafety` |
| Testes automáticos (14) | `ArrowPuzzle.LevelGeneration.Tests` |
| CLI para exportar JSON | `ArrowPuzzle.LevelGeneration.Cli` |

---

## O que você precisa fazer **agora**

**Nada**, se só quiser continuar jogando o 001 manual.

Opcional (só para **experimentar** um nível gerado):

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle

dotnet test services/LevelGeneration/ArrowPuzzle.LevelGeneration.Tests/ArrowPuzzle.LevelGeneration.Tests.csproj

dotnet run --project services/LevelGeneration/ArrowPuzzle.LevelGeneration.Cli -- --level 2 --seed 42 --output .\tmp-002.json
```

Abra `tmp-002.json` e compare com o formato do `001.json`. **Ainda não** substitua o 001 nem suba Docker por causa disso.

---

## Integração nível 002 (feito)

1. **`002.json`** em `LevelCatalog.Infrastructure/Data/levels/` (gerado, seed 253, 4 setas, 5×8).
2. **App:** login → escolher 001 ou 002; in-game botões 001/002 e reiniciar.
3. **Rebuild Docker** necessário para o celular ver o 002:  
   `docker compose up -d --build level-catalog gateway`

## Próximos níveis (003+)

Gerar com CLI, colocar JSON em `Data/levels/`, rebuild `level-catalog`, adicionar botão no app (ou lista dinâmica futura).

---

## Decisão 3.4 (fechada)

- **Padrão:** `BacktrackOnFailedExitTest`  
- **Reserva de corredor:** disponível via enum, não usada no CLI nem nos defaults.

Detalhes técnicos e como reverter: [LEVEL_GENERATION_REVERT.md](LEVEL_GENERATION_REVERT.md).
