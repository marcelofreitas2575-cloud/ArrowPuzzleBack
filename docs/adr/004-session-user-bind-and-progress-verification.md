# ADR-004: Sessão ligada ao usuário e conclusão de nível verificada

- **Status:** Aceita
- **Data:** 2026-10-27

## Contexto

Após introduzir login OIDC (convidado ou futuro Google), era possível:

- Criar sessão de jogo sem amarrar de forma consistente ao **`sub`** do token.
- Marcar nível como completo (`POST /api/v1/me/progress/complete`) **sem** o servidor ter registrado vitória na sessão — incluindo após restart do DynamoDB Local (sessão perdida, `sessionId` stale → 404).

O cliente Android passou a enviar **`sessionId`** no complete para alinhar com a verificação no BFF.

## Decisão

1. **Create session / apply move / get session** exigem JWT; Game Session persiste **dono** (`userId` = `sub`) e valida acesso (`SessionAccessDeniedException` → 403).
2. **`GetSession`** reconcilia **`isPuzzleComplete`** com `PuzzleRules.IsPuzzleComplete(board)` e persiste se o flag estiver desatualizado.
3. **`CompleteLevel`** no Gateway:
   - Com **`sessionId`**: carrega sessão, confere dono, nível e vitória antes de chamar Player Progress.
   - **Sem `sessionId`**: caminho **legacy** (compatível com clientes antigos) — grava progresso sem verificação de sessão.
4. Contrato proto/gRPC/Dapr inclui `user_id` onde aplicável; DynamoDB document guarda owner na sessão.

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| Complete só com `levelId` (anterior) | Simples | Permite trapaça; 404 opaco quando sessão sumiu |
| Obrigar `sessionId` sempre | Seguro | Quebra clientes antigos sem rebuild |
| Verificação condicional + legacy (escolhida) | Seguro para app novo; tolerância dev | Dois caminhos no handler — documentados e testados |

## Consequências

### Positivas

- Progresso reflete vitória **server-side** para fluxo Android atual.
- Testes de Application (Gateway + Game Session) cobrem deny e complete verificado.
- E2E CI simula guest token → solver → moves → complete → progress.

### Negativas / trade-offs

- Sessões em DynamoDB Local são **efêmeras** após `docker compose down -v`; usuário precisa nova sessão (não é bug de auth).
- Legacy path sem `sessionId` ainda existe — remover quando não houver clientes antigos.

## Referências

- [ADR-001: Local first + Keycloak](001-local-first-keycloak.md)
- Gateway: `CompleteLevelHandler`, `GatewayEndpoints`
- Android: `CompleteLevelRequest`, `AppViewModel.onPuzzleWon`
