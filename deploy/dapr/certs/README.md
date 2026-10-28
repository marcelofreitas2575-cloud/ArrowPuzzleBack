# Dapr mTLS (dev local)

No Compose, o serviço **`sentry-certs-init`** gera CA/issuer no volume Docker `dapr_sentry_certs` (funciona no Windows sem erro de permissão).

Para regenerar do zero:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose down -v
docker compose up -d
```

Script opcional (certificados no host, só referência):

```powershell
c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\dapr\certs\generate-sentry-certs.ps1
```

**Somente desenvolvimento** — em produção use PKI gerenciada.
