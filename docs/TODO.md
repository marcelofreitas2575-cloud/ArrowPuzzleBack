# Pendências técnicas — status portfolio

## ✅ Fechado (100% checklist local / documentação)

| Bloco | Itens |
|-------|--------|
| **Backend + CI** | E2E Gateway, correlation, OpenAPI/Postman, Prometheus `/metrics`, Android CI job, Compose LAN |
| **Opção B (Git)** | `main` sincronizada com `origin` |
| **Opção A (Docs)** | README + badge CI, C4, ADR 003–004, [ROADMAP.md](ROADMAP.md) |

Referência operacional: [deploy/docker/README.md](../deploy/docker/README.md), [docs/api/README.md](api/README.md).

---

## Evolução futura (não bloqueia portfolio)

Ver **[ROADMAP.md](ROADMAP.md)** — OTLP/Grafana, Dapr mTLS no K8s, Cognito/AWS CD, testes instrumentados Android.

---

## CI (referência)

Workflow: [`.github/workflows/dotnet-ci.yml`](../.github/workflows/dotnet-ci.yml) — `build-and-test`, `gateway-e2e`, `compose-config`, `android-assemble-debug`.

Badge no [README.md](../README.md).
