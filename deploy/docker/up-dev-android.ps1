# Dev + celular: stack completo no Docker (mesmo comando de sempre).
$ErrorActionPreference = "Stop"
$ComposeDir = "c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker"

Set-Location $ComposeDir
docker compose up -d --build

Write-Host ""
Write-Host "Gateway: http://<IP-da-PC>:5100/health"
Write-Host "Firewall (Admin, uma vez): powershell -ExecutionPolicy Bypass -File $ComposeDir\open-firewall-lan.ps1"
