# C4 — Nível 2 (Containers)

Runtime local via Docker Compose (`deploy/docker/docker-compose.yml`).

```mermaid
flowchart TB
  subgraph client [Cliente]
    Android[Android App]
  end

  subgraph compose [Docker Compose]
    GW[Gateway BFF HTTP 5100]
    GWD[dapr sidecar gateway]
    GS[Game Session gRPC 5101]
    GSD[dapr sidecar game-session]
    LC[Level Catalog gRPC 5102]
    LCD[dapr sidecar level-catalog]
    PP[Player Progress gRPC 5103]
    PPD[dapr sidecar player-progress]
    KC[Keycloak OIDC 8080]
    DDB[(DynamoDB Local 8000)]
    PL[Dapr placement]
  end

  Android -->|HTTPS JWT| GW
  GW --- GWD
  GWD -->|Dapr invoke gRPC| GSD
  GWD -->|Dapr invoke gRPC| LCD
  GWD -->|Dapr invoke gRPC| PPD
  GSD --- GS
  LCD --- LC
  PPD --- PP
  GS --> DDB
  PP --> DDB
  Android -->|token password grant dev| KC
  GW -->|JWKS| KC
```

| Container | Papel |
|-----------|--------|
| **Gateway** | BFF REST, JWT, MediatR, invoca serviços via **Dapr** (padrão) ou gRPC direto (fallback) |
| **Game Session** | Sessão e regras do puzzle |
| **Level Catalog** | Níveis procedural por ID numérico |
| **Player Progress** | Progresso por `sub` OIDC |
| **Keycloak** | OIDC local (realm `arrow-puzzle`) |
| **DynamoDB Local** | Sessões e progresso |

Nome comercial na UI: variável `Product__DisplayName` no gateway (ver README).
