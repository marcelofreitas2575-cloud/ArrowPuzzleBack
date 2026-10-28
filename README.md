# Arrow Puzzle

[![.NET CI](https://github.com/marcelofreitas2575-cloud/ArrowPuzzleBack/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/marcelofreitas2575-cloud/ArrowPuzzleBack/actions/workflows/dotnet-ci.yml)

Jogo de puzzle em grade 2D: setas deslizam até sair ou colidir; vitória ao esvaziar o tabuleiro.

**Nome comercial:** [`docs/PRODUCT_DISPLAY_NAME.md`](docs/PRODUCT_DISPLAY_NAME.md) (único lugar para renomear o produto na UI).

## Reprodução rápida (~5 min)

**Pré-requisitos:** Docker Desktop, [.NET SDK 10](https://dotnet.microsoft.com/download) ([`global.json`](global.json)).

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
copy .env.example .env
docker compose up -d --build
```

1. Abra `http://127.0.0.1:5100/health` → `healthy`.
2. API interativa: `http://127.0.0.1:5100/scalar` → **Dev token** (`POST /api/v1/auth/dev-token`, user `guest-player`) → Authorize → testar `/api/v1/sessions`.
3. (Opcional) Postman: importe [`postman/collections/arrow-puzzle-bff/`](postman/collections/arrow-puzzle-bff/) + [`postman/environments/local.environment.yaml`](postman/environments/local.environment.yaml).

**Celular (mesma Wi‑Fi):** ajuste `DEV_HOST` no `.env` e `dev.host` em `android/local.properties`, rebuild APK — [deploy/docker/README.md](deploy/docker/README.md).

**Testes locais:**

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle
dotnet test ArrowPuzzle.sln -c Release --filter "Category!=E2e"
$env:GATEWAY_E2E_URL = "http://127.0.0.1:5100"
dotnet test tests\ArrowPuzzle.Gateway.E2e.Tests\ArrowPuzzle.Gateway.E2e.Tests.csproj -c Release --filter "Category=E2e"
```

## Stack (visão geral)

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 10, microsserviços hexagonais, MediatR, gRPC (interno / overlay), Polly |
| BFF | REST + JWT; invocação **Dapr HTTP** + JSON Protobuf (padrão Compose) |
| Auth (local) | Keycloak OIDC — migrável para Cognito ([ADR-001](docs/adr/001-local-first-keycloak.md)) |
| Infra local | Docker Compose, DynamoDB Local, Dapr sidecars |
| Mobile | Kotlin, Jetpack Compose, MVVM → Gateway REST |
| Observabilidade | `X-Correlation-Id`, Prometheus `GET /metrics` |
| CI | build/test .NET, Compose validate, gateway E2E, Android `assembleDebug` |

## Documentação

| Tópico | Link |
|--------|------|
| Nome comercial | [docs/PRODUCT_DISPLAY_NAME.md](docs/PRODUCT_DISPLAY_NAME.md) |
| ADRs | [docs/adr/](docs/adr/) — [001 Keycloak](docs/adr/001-local-first-keycloak.md), [002 Dapr + catálogo](docs/adr/002-procedural-catalog-and-dapr.md), [003 Dapr HTTP BFF](docs/adr/003-bff-dapr-http-proto-invoke.md), [004 Sessão + progresso](docs/adr/004-session-user-bind-and-progress-verification.md) |
| C4 (containers) | [docs/c4/containers.md](docs/c4/containers.md) |
| API BFF (OpenAPI / Postman) | [docs/api/README.md](docs/api/README.md) |
| Docker / LAN | [deploy/docker/README.md](deploy/docker/README.md) |
| Roadmap | [docs/ROADMAP.md](docs/ROADMAP.md) |
| Pendências históricas | [docs/TODO.md](docs/TODO.md) |

## Estrutura do repositório

```text
services/          Microsserviços (.NET, hexagonal)
shared/            Protos gRPC e BuildingBlocks
deploy/            Docker Compose, Keycloak, Dapr config
android/           Cliente Kotlin (Compose)
docs/              ADR, C4, API, roadmap
postman/           Coleção v3 do BFF
tests/             E2E Gateway (Compose)
```

Detalhes: [`services/README.md`](services/README.md), [`shared/README.md`](shared/README.md).

## Build (.NET)

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle
dotnet restore ArrowPuzzle.sln
dotnet build ArrowPuzzle.sln -c Release
dotnet test ArrowPuzzle.sln -c Release --filter "Category!=E2e"
```

## Android

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\android
.\gradlew.bat assembleDebug
```

APK: `android/app/build/outputs/apk/debug/app-debug.apk`. Reinstale após mudar `dev.host` ou contrato de API.

## Problemas comuns

| Sintoma | Verificar |
|---------|-----------|
| 401 no app | `guest-player` (bootstrap Keycloak); token via dev-token |
| Connection refused :5100 | `docker compose ps` — gateway Up; rebuild se crashou |
| Complete nível falha | APK recente com `sessionId`; sessão válida após restart Compose |
| LAN | Mesma Wi‑Fi, firewall, `DEV_HOST` / `dev.host` alinhados — [FIX-LAN-CELULAR.md](deploy/docker/FIX-LAN-CELULAR.md) |

## Estratégia

- **Local first** — ver [docs/ROADMAP.md](docs/ROADMAP.md) para AWS, OTLP e Dapr mTLS em K8s.
- **AWS / Cognito:** avaliação futura; BFF agnóstico ao issuer.

## Licença

A definir.
