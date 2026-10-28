# Celular na Wi‑Fi: sobe Docker (sem container gateway) + BFF no Windows (:5100).
# O Docker Desktop publica 5100 via com.docker.backend — o celular muitas vezes não alcança.
#
#   powershell -ExecutionPolicy Bypass -File c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\up-celular.ps1
#
# Deixe a janela do Gateway aberta. Teste no celular: http://<IP-da-PC>:5100/health

$ErrorActionPreference = "Stop"
$ComposeDir = "c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker"

Write-Host "Subindo microsserviços + Dapr + Keycloak (sem container gateway)..."
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-ExecutionPolicy", "Bypass",
    "-File", "$ComposeDir\run-gateway-lan.ps1"
)

Write-Host ""
Write-Host "Abriu uma janela com o Gateway (dotnet). Aguarde 'Now listening on http://0.0.0.0:5100'."
Write-Host "Confira o IP Wi‑Fi: ipconfig"
Write-Host "Celular: http://<seu-ip>:5100/health"
Write-Host ""
Write-Host "Para voltar ao BFF só no Docker: Ctrl+C na janela do Gateway, depois:"
Write-Host "  cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker"
Write-Host "  docker compose -f docker-compose.yml up -d --force-recreate gateway gateway-dapr"
