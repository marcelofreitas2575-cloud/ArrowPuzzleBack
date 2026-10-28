# Certificados DEV (EC) — referência local. O Compose usa sentry-certs-init + volume Docker.
$ErrorActionPreference = "Stop"
$issuerDir = Join-Path $PSScriptRoot "issuer"
New-Item -ItemType Directory -Force -Path $issuerDir | Out-Null

docker run --rm --entrypoint sh `
  -v "${issuerDir}:/certs" `
  alpine/openssl -c @"
openssl ecparam -name prime256v1 -genkey -noout -out /certs/issuer.key
openssl req -new -x509 -days 3650 -key /certs/issuer.key -out /certs/ca.crt -subj '/CN=arrowpuzzle-dapr-ca'
cp /certs/ca.crt /certs/issuer.crt
"@

Write-Host "OK: $issuerDir"
