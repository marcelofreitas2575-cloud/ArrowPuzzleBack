# Roadmap técnico — Arrow Puzzle

Documento de evolução **após** o stack local-first atual (Compose + Keycloak + Dapr HTTP + Android). Nada aqui é compromisso de prazo; prioridade segue decisão explícita em TODO AWS.

## Entregue (baseline portfolio)

| Área | Estado |
|------|--------|
| Compose + Dapr sidecars + BFF LAN :5100 | ✅ |
| OIDC Keycloak + dev-token | ✅ |
| Sessão/progresso DynamoDB Local | ✅ |
| Catálogo procedural solvável | ✅ |
| CI: .NET test, Compose config, gateway E2E, Android `assembleDebug` | ✅ |
| Contrato BFF: OpenAPI, Scalar, Postman v3 | ✅ |
| Observabilidade: `X-Correlation-Id`, Prometheus `/metrics` | ✅ |

## Próximas ondas (ordenadas sugerida)

### 1. Observabilidade avançada

- Export **OTLP** (traces) para Jaeger ou Grafana Tempo.
- Dashboards **RED** prontos (Grafana) a partir de `/metrics` ou OTLP metrics.
- Manter correlation ID como chave de join logs ↔ traces.

### 2. Dapr em ambiente “real”

- **mTLS** e **ACL** entre apps — alvo **Kubernetes** ou VM com Dapr 1.15+; **fora** do Compose de dev (certificados locais só referência em `deploy/dapr/certs/`).
- Sentry/issuer gerenciado por PKI, não scripts dev.

### 3. Identidade cloud

- Migrar **Keycloak → Amazon Cognito** (ou manter Keycloak em EC2) conforme [ADR-001](adr/001-local-first-keycloak.md).
- BFF permanece agnóstico: `Authority`, audience, JWKS por config.

### 4. Persistência e catálogo cloud

- DynamoDB + S3 (ou equivalente) para sessão/progresso/catálogo quando sair do Local.
- Pipeline de publicação de níveis com validação solver (já espelhada em `ProceduralLevelSolvabilityGuard`).

### 5. CD / AWS

- Imagens → ECR; deploy EC2/EKS ou App Runner — **TODO** explícito no README principal.
- Terraform em `deploy/terraform/` quando a conta e o desenho forem aprovados.

## Explicitamente fora de escopo (por ora)

- JSON estático de níveis no repositório como catálogo principal.
- Gateway `network_mode` com sidecar (legado archive).
- Testes instrumentados Android em CI (opcional futuro).

## Referências

- [docs/TODO.md](TODO.md) — histórico de pendências
- [docs/adr/](adr/) — decisões arquiteturais
- [deploy/docker/README.md](../deploy/docker/README.md) — operação local
