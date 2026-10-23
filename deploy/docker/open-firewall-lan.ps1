# Libera LAN → gateway :5100 (Docker Desktop + Kestrel no host).
# Execute como Administrador:
#   powershell -ExecutionPolicy Bypass -File c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\open-firewall-lan.ps1
#
# Referência: Docker encaminha portas via com.docker.backend.exe; em Wi‑Fi Privada
# a regra padrão "Docker Desktop Backend" costuma valer só para Perfil Público.
# https://docs.docker.com/desktop/features/networking/

$ErrorActionPreference = "Stop"
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error @"
Execute como Administrador (obrigatório uma vez):
  powershell -ExecutionPolicy Bypass -File c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\open-firewall-lan.ps1
"@
    exit 1
}

$profiles = @("Domain", "Private", "Public")
$dockerBackend = "${env:ProgramFiles}\Docker\Docker\resources\com.docker.backend.exe"

function Ensure-PortRule {
    param([string]$Name, [int]$Port)
    $existing = Get-NetFirewallRule -DisplayName $Name -ErrorAction SilentlyContinue
    if ($existing) {
        Set-NetFirewallRule -DisplayName $Name -Profile $profiles -Enabled True -Action Allow | Out-Null
    } else {
        New-NetFirewallRule `
            -DisplayName $Name `
            -Direction Inbound `
            -Action Allow `
            -Protocol TCP `
            -LocalPort $Port `
            -Profile $profiles | Out-Null
    }
    Write-Host "OK porta TCP ${Port}: $Name (Domain, Private, Public)"
}

function Ensure-ProgramAllowRule {
    param([string]$Name, [string]$ProgramPath)
    if (-not (Test-Path -LiteralPath $ProgramPath)) {
        Write-Host "SKIP (arquivo não existe): $ProgramPath"
        return
    }
    $existing = Get-NetFirewallRule -DisplayName $Name -ErrorAction SilentlyContinue
    if ($existing) {
        Set-NetFirewallRule -DisplayName $Name -Profile $profiles -Enabled True -Action Allow | Out-Null
    } else {
        New-NetFirewallRule `
            -DisplayName $Name `
            -Direction Inbound `
            -Action Allow `
            -Program $ProgramPath `
            -Profile $profiles | Out-Null
    }
    Write-Host "OK programa: $Name"
}

Ensure-PortRule -Name "Arrow Puzzle Gateway TCP 5100" -Port 5100

$legacy = Get-NetFirewallRule -DisplayName "Arrow Puzzle BFF 5100" -ErrorAction SilentlyContinue
if ($legacy) {
    Set-NetFirewallRule -DisplayName "Arrow Puzzle BFF 5100" -Profile $profiles -Enabled True | Out-Null
}

Ensure-ProgramAllowRule -Name "Arrow Puzzle Docker Backend LAN" -ProgramPath $dockerBackend

$gatewayExe = "c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\services\Gateway\ArrowPuzzle.Gateway.Api\bin\Release\net10.0\ArrowPuzzle.Gateway.Api.exe"
Ensure-ProgramAllowRule -Name "Arrow Puzzle Gateway Api LAN" -ProgramPath $gatewayExe

# Wi‑Fi doméstica: Perfil Privado (Public costuma ser mais restritivo e confunde regras do Docker).
$wifi = Get-NetConnectionProfile -ErrorAction SilentlyContinue | Where-Object { $_.InterfaceAlias -match "Wi-Fi" -and $_.IPv4Connectivity -eq "Internet" } | Select-Object -First 1
if ($wifi) {
    if ($wifi.NetworkCategory -ne "Private") {
        Set-NetConnectionProfile -InterfaceAlias $wifi.InterfaceAlias -NetworkCategory Private
        Write-Host "Wi-Fi '$($wifi.Name)' → Perfil Privado (recomendado para LAN dev)."
    } else {
        Write-Host "Wi-Fi '$($wifi.Name)' já está em Perfil Privado."
    }
}

Write-Host ""
Write-Host "Celular (Chrome): http://192.168.0.38:5100/health"
Write-Host "Confirme o IPv4 da PC com: ipconfig"
