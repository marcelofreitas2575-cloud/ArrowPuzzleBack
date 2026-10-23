# Opcional / diagnóstico: BFF no host se a porta 5100 no Docker falhar.
# Fluxo normal: docker compose up -d (gateway + gateway-dapr no Compose; sidecar sem network_mode:service:gateway).

$ErrorActionPreference = "Stop"
$RepoRoot = "c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle"
$ComposeDir = "$RepoRoot\deploy\docker"

Set-Location $ComposeDir
docker compose stop gateway gateway-dapr 2>$null | Out-Null
docker compose rm -f gateway gateway-dapr 2>$null | Out-Null

$listener = Get-NetTCPConnection -LocalPort 5100 -State Listen -ErrorAction SilentlyContinue
if ($listener) {
    $proc = Get-Process -Id $listener.OwningProcess -ErrorAction SilentlyContinue
    if ($proc -and $proc.ProcessName -notmatch "dotnet") {
        Write-Host "Porta 5100 em uso por $($proc.ProcessName) (PID $($proc.Id)). Pare o processo ou o container gateway."
        exit 1
    }
}

Set-Location $RepoRoot
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://0.0.0.0:5100"
$env:Dapr__Enabled = "false"

Write-Host "Gateway LAN: http://0.0.0.0:5100 (health: /health)"
Write-Host "Ctrl+C para parar."

dotnet run --project "$RepoRoot\services\Gateway\ArrowPuzzle.Gateway.Api\ArrowPuzzle.Gateway.Api.csproj" --no-launch-profile
