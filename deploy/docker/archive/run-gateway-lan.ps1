# BFF no Windows em 0.0.0.0:5100 — celular na Wi‑Fi alcança a PC (evita com.docker.backend na LAN).
# Backends + Dapr continuam no Docker. Execute:
#   powershell -ExecutionPolicy Bypass -File c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\run-gateway-lan.ps1

$ErrorActionPreference = "Stop"
$RepoRoot = "c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle"
$ComposeDir = "$RepoRoot\deploy\docker"

Set-Location $ComposeDir

if (Test-Path "$ComposeDir\.env") {
    Get-Content "$ComposeDir\.env" | ForEach-Object {
        if ($_ -match '^\s*DEV_HOST\s*=\s*(.+)\s*$') {
            $env:DEV_HOST = $Matches[1].Trim()
        }
    }
}
if (-not $env:DEV_HOST) { $env:DEV_HOST = "192.168.0.38" }

Write-Host "Parando container gateway (libera 5100 para Kestrel no Windows)..."
docker compose stop gateway 2>$null | Out-Null
docker compose rm -f gateway 2>$null | Out-Null

Write-Host "Subindo stack + gateway-dapr -> host.docker.internal ..."
docker compose -f docker-compose.yml -f docker-compose.lan-host.yml up -d --build `
    keycloak postgres dynamodb placement `
    game-session level-catalog player-progress `
    game-session-dapr level-catalog-dapr player-progress-dapr `
    gateway-dapr keycloak-guest-bootstrap 2>&1 | Out-Host

$listeners = Get-NetTCPConnection -LocalPort 5100 -State Listen -ErrorAction SilentlyContinue
foreach ($listener in $listeners) {
    $proc = Get-Process -Id $listener.OwningProcess -ErrorAction SilentlyContinue
    if (-not $proc) { continue }
    if ($proc.ProcessName -match "dotnet|ArrowPuzzle") { continue }
    Write-Host "Porta 5100 em uso por $($proc.ProcessName) (PID $($proc.Id), bind $($listener.LocalAddress))."
    Write-Host "Pare o Docker gateway ou feche o processo antes de continuar."
    exit 1
}

Set-Location $RepoRoot
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5100"
$env:Dapr__Enabled = "true"
$env:DAPR_HTTP_ENDPOINT = "http://127.0.0.1:3500"
$env:DAPR_GRPC_ENDPOINT = "http://127.0.0.1:50001"
$env:KeycloakDevAuth__TokenEndpointUrl = "http://localhost:8080/realms/arrow-puzzle/protocol/openid-connect/token"
$env:KeycloakDevAuth__Enabled = "true"
$env:Authentication__Jwt__Authority = "http://localhost:8080/realms/arrow-puzzle"
$env:Authentication__Jwt__MetadataAddress = "http://localhost:8080/realms/arrow-puzzle"
$env:Authentication__Jwt__JwksUri = "http://localhost:8080/realms/arrow-puzzle/protocol/openid-connect/certs"
$env:Authentication__Jwt__RequireHttpsMetadata = "false"
$env:Authentication__Jwt__ValidIssuers__0 = "http://localhost:8080/realms/arrow-puzzle"
$env:Authentication__Jwt__ValidIssuers__1 = "http://127.0.0.1:8080/realms/arrow-puzzle"
$env:Authentication__Jwt__ValidIssuers__2 = "http://$($env:DEV_HOST):8080/realms/arrow-puzzle"
$env:Authentication__Jwt__ValidIssuers__3 = "http://10.0.2.2:8080/realms/arrow-puzzle"
# keycloak:8080 só resolve dentro do Docker; no host usamos localhost/LAN.

Write-Host ""
Write-Host "Gateway LAN (Kestrel Windows): http://0.0.0.0:5100"
Write-Host "Celular Chrome: http://$($env:DEV_HOST):5100/health"
Write-Host "Ctrl+C para parar. Depois: docker compose up -d gateway  (volta BFF no Docker)"
Write-Host ""

dotnet run --project "$RepoRoot\services\Gateway\ArrowPuzzle.Gateway.Api\ArrowPuzzle.Gateway.Api.csproj" --no-launch-profile
