# ADR-003: BFF → microsserviços via Dapr HTTP e JSON Protobuf

- **Status:** Aceita
- **Data:** 2026-10-27

## Contexto

No Compose local, o **Gateway (BFF)** orquestra Game Session, Level Catalog e Player Progress. Precisávamos de:

- Um caminho único de invocação alinhado ao runtime Dapr (sidecars, placement, resiliência futura).
- Contratos estáveis (`shared/ArrowPuzzle.Protos`) sem expor gRPC diretamente ao Android.
- Observabilidade: propagar `X-Correlation-Id` nas chamadas saindo do BFF.

O overlay `docker-compose.direct-grpc.yml` existe só para **diagnóstico** quando sidecars falham.

## Decisão

1. Com **`Dapr__Enabled: true`**, o BFF invoca backends via **Dapr service invocation HTTP**: rotas `/dapr/*` nos apps ASP.NET, corpo **JSON serializado a partir de mensagens Protobuf** (`DaprProtoHttpInvoke` / `DaprHttpProtoInvoke` em BuildingBlocks).
2. Sidecars usam `-app-channel-address` apontando para o container do serviço (sem `network_mode: service:gateway`), preservando **`0.0.0.0:5100`** para LAN/celular.
3. Middleware opcional **`DAPR_APP_API_TOKEN`** valida chamadas às rotas `/dapr/*` nos microsserviços (dev local via `.env`).
4. **InternalGrpc** permanece **desligado por padrão** (`InternalGrpc__Enabled: false`); gRPC h2c só quando o overlay direct-grpc ou config explícita o exige.

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| gRPC direto BFF → serviços (padrão) | Menos moving parts | Sidecars ociosos; LAN/Dapr mDNS mais frágil no Desktop |
| Dapr HTTP + JSON proto (escolhida) | Um padrão no Compose; mesmo contrato proto; fácil de testar com curl/Postman interno | Camada extra de serialização; sidecars em dev |
| REST hand-written por serviço | Simples de ler | Duplica contrato proto; drift com gRPC interno |

## Consequências

### Positivas

- Android fala só **REST + JWT** com o BFF; microsserviços permanecem isolados na rede Compose.
- Correlation ID propagado no invoke HTTP do BFF.
- CI E2E valida o caminho Dapr ponta a ponta (`docker compose up` + guest win).

### Negativas / trade-offs

- mTLS/ACL Dapr **não** configurados no Compose de dev (ver [docs/ROADMAP.md](../ROADMAP.md)).
- Depuração exige entender sidecar + app channel, não apenas uma porta gRPC.

## Referências

- [ADR-002: Catálogo procedural e Dapr no BFF](002-procedural-catalog-and-dapr.md)
- [docs/c4/containers.md](../c4/containers.md)
- [deploy/docker/docker-compose.yml](../../deploy/docker/docker-compose.yml)
