# Arrow Puzzle

Jogo de puzzle em grade 2D: setas deslizam até sair ou colidir; vitória ao esvaziar o tabuleiro.

**Nome comercial:** definido em [`docs/PRODUCT_DISPLAY_NAME.md`](docs/PRODUCT_DISPLAY_NAME.md) (único local do repositório para renomear o produto sem alterar código ou infra).

## Stack (visão geral)

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 10, microsserviços, hexagonal, gRPC, MediatR, Polly |
| Auth (local) | Keycloak (OIDC) — preparado para migrar para Cognito |
| Infra local | Docker Compose, DynamoDB Local (fases posteriores), Dapr (fase posterior) |
| Mobile | Kotlin, Jetpack Compose, MVVM |

## Estratégia

- **Local first:** runtime de desenvolvimento e demonstração via Docker Compose em ambiente local.
- **Cliente Android:** integração com backend via gRPC na LAN (host e dispositivo no **mesmo Wi‑Fi**).
- **AWS / Cognito:** TODO — avaliação futura (custo operacional, requisitos de disponibilidade contínua).

## Documentação

- Nome comercial (renomeável): [`docs/PRODUCT_DISPLAY_NAME.md`](docs/PRODUCT_DISPLAY_NAME.md)
- Decisões de arquitetura: [`docs/adr/`](docs/adr/)
  - [ADR-001: Local first + Keycloak (OIDC)](docs/adr/001-local-first-keycloak.md)
  - [ADR-002: Catálogo procedural + Dapr no BFF](docs/adr/002-procedural-catalog-and-dapr.md)
  - Template para novas ADRs: [000-template.md](docs/adr/000-template.md)
- Diagramas C4: [`docs/c4/`](docs/c4/) (documentação de arquitetura).

## Estrutura do repositório

```text
services/          Microsserviços (.NET, hexagonal por pasta)
shared/            Protos gRPC e BuildingBlocks
deploy/            Docker Compose, Keycloak, Terraform (TODO AWS)
android/           Cliente Kotlin (Compose)
docs/              ADR, C4, nome comercial
```

Detalhes: [`services/README.md`](services/README.md), [`shared/README.md`](shared/README.md).

## CI

Push/PR na branch `main` disparam [.github/workflows/dotnet-ci.yml](.github/workflows/dotnet-ci.yml) (build + testes .NET e validação do Compose).

## Build (.NET)

Requisito: [.NET SDK 10](https://dotnet.microsoft.com/download) (versão fixada em [`global.json`](global.json)).

```powershell
dotnet restore ArrowPuzzle.sln
dotnet build ArrowPuzzle.sln
dotnet test ArrowPuzzle.sln
```

## Como rodar (local)

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Celular Android e PC na **mesma Wi‑Fi** (teste físico) ou emulador
- `android/local.properties` com `dev.host=<IP da PC na Wi‑Fi>` (emulador: `10.0.2.2`)

### Backend (Compose)

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
copy .env.example .env
# Ajuste DEV_HOST e opcionalmente PRODUCT_DISPLAY_NAME no .env
docker compose up -d --build
```

Detalhes (overlay gRPC direto, Google IdP, LAN): [deploy/docker/README.md](deploy/docker/README.md).

Portas: **5100** (gateway — app Android), **8080** (Keycloak), **5101–5103** (gRPC). Login convidado via `POST /api/v1/auth/dev-token` no gateway. Stack padrão inclui **Dapr** sidecars.

Título do jogo na UI: variável `PRODUCT_DISPLAY_NAME` → `Product__DisplayName` no gateway ([docs/PRODUCT_DISPLAY_NAME.md](docs/PRODUCT_DISPLAY_NAME.md)).

Health: `http://localhost:5100/health`, `http://localhost:5102/health`, etc.

### Android

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\android
.\gradlew.bat assembleDebug
```

APK: `android/app/build/outputs/apk/debug/app-debug.apk`. Reinstale após mudar `dev.host`.

### Problemas comuns

| Sintoma | Verificar |
|---------|-----------|
| 401 no app | Usuário `guest-player` no Keycloak (`keycloak-guest-bootstrap`); `DEV_HOST` no `.env` igual ao IP usado pelo celular |
| App não alcança API | Mesma Wi‑Fi, firewall 5100/8080, `dev.host` correto no build |

## Licença

A definir.
