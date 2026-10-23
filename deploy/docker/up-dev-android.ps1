# Legado / diagnóstico. Preferir: docker compose up -d --build
$ErrorActionPreference = "Stop"
$ComposeDir = "c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker"

Set-Location $ComposeDir
docker compose stop gateway gateway-dapr 2>$null | Out-Null
docker compose rm -f gateway gateway-dapr 2>$null | Out-Null
docker compose up -d --build postgres keycloak dynamodb game-session level-catalog player-progress

Write-Host ""
Write-Host "Aguardando microsserviços..."
Start-Sleep -Seconds 8
docker compose ps game-session level-catalog player-progress keycloak

Write-Host ""
Write-Host "Iniciando BFF na LAN (nova janela)..."
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-ExecutionPolicy", "Bypass",
    "-File", "$ComposeDir\run-gateway-lan.ps1"
)
