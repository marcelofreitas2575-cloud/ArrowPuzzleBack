# Shared (cross-cutting)

Código e contratos **reutilizados** por vários microsserviços, **sem** regra de negócio de um bounded context específico.

| Pasta | Conteúdo |
|-------|----------|
| [`ArrowPuzzle.Protos/`](ArrowPuzzle.Protos/) | Definições `.proto` (gRPC), versionadas (`v1`) |
| [`ArrowPuzzle.BuildingBlocks/`](ArrowPuzzle.BuildingBlocks/) | Health checks, interceptors gRPC, extensões Polly, logging estruturado |

Nome comercial do jogo **não** entra em packages ou namespaces — ver [`../docs/PRODUCT_DISPLAY_NAME.md`](../docs/PRODUCT_DISPLAY_NAME.md).
