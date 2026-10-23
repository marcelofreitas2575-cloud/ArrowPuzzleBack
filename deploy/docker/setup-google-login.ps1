# Prepara .env e aplica Google IdP no Keycloak (requer credenciais Google no .env).
$ErrorActionPreference = "Stop"
$here = $PSScriptRoot
$envFile = Join-Path $here ".env"
$example = Join-Path $here ".env.example"

if (-not (Test-Path $envFile)) {
    Copy-Item $example $envFile
    Write-Host "Criado $envFile — edite e coloque GOOGLE_IDP_CLIENT_ID e GOOGLE_IDP_CLIENT_SECRET."
    Write-Host "Guia: $(Join-Path $here 'CONFIGURAR_GOOGLE.md')"
    notepad $envFile
    exit 0
}

$lines = Get-Content $envFile
$hasId = $lines | Where-Object { $_ -match '^\s*GOOGLE_IDP_CLIENT_ID=\S+' }
$hasSecret = $lines | Where-Object { $_ -match '^\s*GOOGLE_IDP_CLIENT_SECRET=\S+' }

if (-not $hasId -or -not $hasSecret) {
    Write-Host "Preencha GOOGLE_IDP_CLIENT_ID e GOOGLE_IDP_CLIENT_SECRET em $envFile"
    Write-Host "Guia: $(Join-Path $here 'CONFIGURAR_GOOGLE.md')"
    notepad $envFile
    exit 1
}

Push-Location $here
try {
    docker compose up -d keycloak
    docker compose run --rm keycloak-google-bootstrap
    Write-Host ""
    Write-Host "Pronto. Teste 'Continuar com Google' no app."
    Write-Host "Se falhar no celular, leia Passo 1b (ngrok) em CONFIGURAR_GOOGLE.md"
} finally {
    Pop-Location
}
