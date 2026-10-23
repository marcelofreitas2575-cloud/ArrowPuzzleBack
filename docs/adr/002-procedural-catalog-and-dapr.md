# ADR-002: Catálogo procedural e invocação Dapr no BFF

- **Status:** Aceita
- **Data:** 2026-03-21

## Contexto

- Níveis numéricos (`001`, `002`, …) devem ser sempre solváveis.
- JSON embedded no repositório adicionava complexidade sem uso no produto atual.
- O gateway precisa de um caminho único de invocação entre microsserviços no Compose local.

## Decisão

1. **Level Catalog** expõe apenas níveis **procedural** (gerador + `PuzzleSolver` + guarda `ProceduralLevelSolvabilityGuard` no repositório). Sem JSON embedded nem seed Dynamo para fases fixas.
2. **Gateway (Compose local):** padrão **`Dapr__Enabled: true`** + sidecars no `docker-compose.yml`; `gateway-dapr` em rede própria com `-app-channel-address gateway` (não `network_mode: service:gateway`) para LAN na **5100**.
3. **Fallback:** overlay `docker-compose.direct-grpc.yml` — gRPC direto (`Services__*`) só para diagnóstico.
4. **Nome na UI:** `Product__DisplayName` no gateway; app Android lê `GET /api/v1/product` (sem recompilar o APK ao mudar o título).

## Consequências

- DynamoDB Local não armazena definições de nível; apenas sessão e progresso.
- Kestrel nos serviços gRPC: HTTP/2 (h2c) para gRPC direto; health via `curl --http2-prior-knowledge`.
- Compose padrão: `placement` + sidecars por serviço; invocação BFF → backends via **Dapr HTTP** (`/dapr/*` + JSON Protobuf), com gRPC h2c nos apps para clientes diretos (overlay `direct-grpc`).
- Adaptadores gRPC diretos permanecem no código para overlay `direct-grpc`, não como caminho padrão.

## Referências

- [docs/c4/containers.md](../c4/containers.md)
- [docs/TODO.md](../TODO.md)
