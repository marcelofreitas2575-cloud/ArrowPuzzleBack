# Pendências técnicas (local / roadmap)

## Fechado neste ciclo (backend + CI + contrato)

- **E2E Gateway** — `tests/ArrowPuzzle.Gateway.E2e.Tests`, job `gateway-e2e`, [e2e-wait-gateway.sh](../deploy/docker/e2e-wait-gateway.sh)
- **Correlation** — `X-Correlation-Id` + propagação Dapr no BFF
- **OpenAPI / Postman BFF** — [docs/api/](api/), [postman/collections/arrow-puzzle-bff/](../postman/collections/arrow-puzzle-bff/), runtime `/openapi/v1.json` + `/scalar`
- **Observabilidade RED** — OpenTelemetry + `GET /metrics` (5100–5103)
- **CI Android** — job `android-assemble-debug` em [`.github/workflows/dotnet-ci.yml`](../.github/workflows/dotnet-ci.yml)
- **Compose LAN** — stack padrão em [deploy/docker/docker-compose.yml](../deploy/docker/docker-compose.yml)

---

## Próximo tópico → **Opção A** (documentação portfolio)

| # | Entrega | Onde |
|---|---------|------|
| A1 | README: badge CI, repro ~5 min, links docker + api | Raiz `README.md` |
| A2 | C4 containers atualizado | [docs/c4/containers.md](c4/containers.md) |
| A3 | ADRs: Dapr HTTP BFF; sessão + `complete` verificado | `docs/adr/` |
| A4 | Roadmap (mTLS Dapr K8s, OTLP/Grafana, AWS CD) | `docs/ROADMAP.md` ou README |

**Opção B (Git):** concluída — `main` em `origin` no commit `15f61cc` (fast-forward de `feat/session-observability-and-ci`).

---

## CI/CD (referência)

**Jobs:** `build-and-test`, `gateway-e2e`, `compose-config`, `android-assemble-debug`.

**Pendente leve:** badge no README (A1).

**CD:** AWS — fora do escopo local-first.

## Observabilidade (referência)

**Feito:** correlation + Prometheus RED em cada API.

**Roadmap:** export OTLP (traces), dashboards Grafana — ver A4.

## Docker / Dapr / catálogo / testes / Android

Sem mudança de escopo — ver histórico git e [deploy/docker/README.md](../deploy/docker/README.md).

## Pacotes de trabalho (legado)

### Opção B — Git (após A)

| # | Entrega |
|---|---------|
| B1 | Inventário do diff vs `main` |
| B2 | Commits lógicos (session, obs, test/ci) |
| B3 | Push + PR |
| B4 | Higiene `.gitignore` / sem secrets |

### Opção C — Validar local

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose up -d --build
```

- Browser: `http://127.0.0.1:5100/scalar`, `http://127.0.0.1:5100/metrics`
- E2E: `$env:GATEWAY_E2E_URL="http://127.0.0.1:5100"` + `dotnet test tests\ArrowPuzzle.Gateway.E2e.Tests\ ... --filter Category=E2e`
- Android: rebuild APK se `sessionId` no complete ainda não estiver instalado
