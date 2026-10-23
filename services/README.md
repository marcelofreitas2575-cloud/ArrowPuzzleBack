# Microsserviços (backend)

Cada bounded context possui **quatro projetos** (.NET), organizados em **arquitetura hexagonal**:

| Projeto | Camada | Responsabilidade |
|---------|--------|------------------|
| `ArrowPuzzle.{Service}.Domain` | Núcleo | Entidades, value objects, regras de negócio; sem dependências externas |
| `ArrowPuzzle.{Service}.Application` | Aplicação | Casos de uso (MediatR), ports (interfaces) |
| `ArrowPuzzle.{Service}.Infrastructure` | Adaptadores | DynamoDB, clientes gRPC, integrações |
| `ArrowPuzzle.{Service}.Api` | Entrada | Host ASP.NET Core, gRPC, composição (DI) |

## Serviços

| Pasta | Papel |
|-------|--------|
| [`Gateway/`](Gateway/) | BFF — API pública, JWT OIDC, orquestração |
| [`GameSession/`](GameSession/) | Sessão de jogo e regras do puzzle |
| [`PlayerProgress/`](PlayerProgress/) | Progresso do jogador (`sub` OIDC) |
| [`LevelCatalog/`](LevelCatalog/) | Definição de fases e metadados |

Dependências entre projetos de **um mesmo serviço**: `Api` → `Application` + `Infrastructure`; `Application` → `Domain`; `Infrastructure` → `Application` + `Domain`. **Domain** não referencia outros projetos.

Contratos gRPC compartilhados: [`../shared/ArrowPuzzle.Protos/`](../shared/ArrowPuzzle.Protos/).
