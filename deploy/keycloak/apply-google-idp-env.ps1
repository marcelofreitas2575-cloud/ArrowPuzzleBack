# Atualiza Client ID/Secret do Google IdP no realm importado (dev local).
# Uso (na pasta deploy/docker, com .env contendo GOOGLE_IDP_CLIENT_ID e GOOGLE_IDP_CLIENT_SECRET):
#   ..\\keycloak\\apply-google-idp-env.ps1

param(
    [string]$RealmFile = "$PSScriptRoot\import\arrow-puzzle-realm.json"
)

$clientId = $env:GOOGLE_IDP_CLIENT_ID
$secret = $env:GOOGLE_IDP_CLIENT_SECRET

if ([string]::IsNullOrWhiteSpace($clientId) -or [string]::IsNullOrWhiteSpace($secret)) {
    Write-Host "Defina GOOGLE_IDP_CLIENT_ID e GOOGLE_IDP_CLIENT_SECRET no ambiente ou em deploy/docker/.env"
    exit 1
}

$json = Get-Content -Raw -Path $RealmFile | ConvertFrom-Json
$idp = $json.identityProviders | Where-Object { $_.alias -eq "google" } | Select-Object -First 1
if ($null -eq $idp) {
    Write-Error "Identity provider 'google' não encontrado em $RealmFile"
    exit 1
}

$idp.enabled = $true
$idp.config.clientId = $clientId
$idp.config.clientSecret = $secret

$json | ConvertTo-Json -Depth 20 | Set-Content -Path $RealmFile -Encoding UTF8
Write-Host "Google IdP aplicado em $RealmFile. Recrie o container keycloak (import-realm) para carregar."
