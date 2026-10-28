# Docker Compose — Arrow Puzzle (local)

## Subir tudo (PC + celular)

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
copy .env.example .env
docker compose up -d --build
```

Postgres, Keycloak (+ guest bootstrap), DynamoDB Local, microsserviços, **Dapr** (placement + sidecars), gateway BFF na **5100**.

### BFF — API, docs e métricas (host)

| URL | Descrição |
|-----|-----------|
| `http://127.0.0.1:5100/health` | Saúde do gateway |
| `http://127.0.0.1:5100/scalar` | UI OpenAPI (Scalar) |
| `http://127.0.0.1:5100/openapi/v1.json` | Contrato OpenAPI JSON |
| `http://127.0.0.1:5100/metrics` | Prometheus (RED HTTP) |

Contrato estático + Postman: [docs/api/README.md](../../docs/api/README.md).

Parar:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose down --remove-orphans
```

## Celular (mesma Wi‑Fi)

1. `dev.host=<IPv4 da PC>` em `android/local.properties` → rebuild APK.
2. Firewall (Admin, uma vez):

```powershell
powershell -ExecutionPolicy Bypass -File c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\open-firewall-lan.ps1
```

3. Chrome: `http://<IP-da-PC>:5100/health`

Detalhes: [FIX-LAN-CELULAR.md](FIX-LAN-CELULAR.md).

## Overlays opcionais

- **debug-ports:** `docker-compose.debug-ports.yml`
- **gRPC direto no BFF (sem Dapr):** `docker-compose.direct-grpc.yml`

## Google IdP

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose --profile google-idp up -d keycloak-google-bootstrap
```

Ver [deploy/keycloak/GOOGLE_IDP.md](../keycloak/GOOGLE_IDP.md).
