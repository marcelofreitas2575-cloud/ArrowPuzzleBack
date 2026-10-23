# Docker Compose — Arrow Puzzle (local)

## Stack padrão (dev + celular)

Um único arquivo principal:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
copy .env.example .env
docker compose up -d --build
```

Inclui: Postgres + Keycloak (+ guest bootstrap), DynamoDB Local, microsserviços gRPC, **Dapr** (placement + sidecars), gateway BFF na **`0.0.0.0:5100`**.

Parar:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose down --remove-orphans
```

## Overlay de diagnóstico (gRPC direto no gateway)

Sem invocação Dapr no BFF — só para isolar falhas de sidecar/Dapr:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose -f docker-compose.yml -f docker-compose.direct-grpc.yml up -d --build gateway
```

## Google IdP no Keycloak (opcional)

Com credenciais no `.env` / ambiente:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose --profile google-idp up -d keycloak-google-bootstrap
```

Ver [deploy/keycloak/GOOGLE_IDP.md](../keycloak/GOOGLE_IDP.md).

## LAN / celular

- Firewall: [open-firewall-lan.ps1](open-firewall-lan.ps1)
- Troubleshooting: [FIX-LAN-CELULAR.md](FIX-LAN-CELULAR.md), [CONNECTION_TEST.md](CONNECTION_TEST.md)
- BFF no host (último recurso): [run-gateway-lan.ps1](run-gateway-lan.ps1)

## Arquivos legados

Compose antigos (connection-test, Keycloak duplicado, overlay Dapr vazio) estão em [archive/](archive/README.md).
