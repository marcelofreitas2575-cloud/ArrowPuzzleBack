# Pendências técnicas (local / roadmap)

## CI/CD

**Status (fase 1):** GitHub Actions [`.github/workflows/dotnet-ci.yml`](../.github/workflows/dotnet-ci.yml) — `dotnet restore/build/test` + `docker compose config` em push/PR na `main`.

**Próximo:** badge no README; Android `assembleDebug` (opcional, job separado ou só `main`); E2E Compose (self-hosted runner ou manual).

**CD:** após decisão AWS — build imagens, ECR, deploy (fora do escopo local-first).

## Docker Compose (organização)

**Status:** feito — stack em [deploy/docker/docker-compose.yml](../deploy/docker/docker-compose.yml); overlay de diagnóstico [docker-compose.direct-grpc.yml](../deploy/docker/docker-compose.direct-grpc.yml); legados em [deploy/docker/archive/](../deploy/docker/archive/). Guia: [deploy/docker/README.md](../deploy/docker/README.md).

## Dapr no Gateway (Compose local)

**Status:** padrão — sidecars em [deploy/docker/docker-compose.yml](../deploy/docker/docker-compose.yml); gateway com `Dapr__Enabled: true` e `DAPR_*_ENDPOINT` apontando para `gateway-dapr` (sidecar **sem** `network_mode: service:gateway`, para preservar `0.0.0.0:5100` na LAN).

**Fallback diagnóstico:** overlay [deploy/docker/docker-compose.direct-grpc.yml](../deploy/docker/docker-compose.direct-grpc.yml) (`Services__*` + `Dapr__Enabled: false`).

**Critério de pronto:** `docker compose up` + E2E dev-token + `POST /api/v1/sessions` (nível `001`); celular `http://<IP-PC>:5100/health`.

**Nota técnica:** invocação BFF → serviços via rotas HTTP `/dapr/*` (JSON Protobuf); sidecars com `-app-channel-address` (sem `network_mode: service:*`) para LAN na 5100 e mDNS estável.

## Catálogo procedural

**Status:** Level Catalog gera níveis numéricos (`001`, `002`, …) **somente procedural** — sem JSON embedded/S3 no fluxo do app.

**Pipeline de qualidade (implementado):**

1. `ProceduralLevelGenerator` — só aceita layout com `PuzzleSolver.IsSolvable` na geração.
2. `ProceduralLevelSolvabilityGuard` — antes de devolver ao gRPC, roda `IsSolvable` + `TrySolve` independente.
3. Testes — amostra de IDs + níveis `001`–`040` no CI (`ArrowPuzzle.LevelCatalog.Infrastructure.Tests`).

**Fora de escopo:** arquivos JSON de nível no repositório ou catálogo estático.

## Player Progress (Android)

**Status:** app usa `GET/POST /api/v1/me/progress` (convidado OIDC → DynamoDB via Player Progress). `LocalProgressStore` só cache do nível jogável.
