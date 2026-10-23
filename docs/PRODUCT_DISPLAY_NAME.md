# Nome comercial do jogo

Referência para marketing e valor **padrão** do título na UI.

| Campo | Valor sugerido |
|-------|----------------|
| **Nome completo** | Arrow Puzzle |
| **Título na UI (Android)** | configurável em runtime |

## Alterar o título sem recompilar o APK

1. No PC, edite `deploy/docker/.env` (ou variáveis do Compose):

   ```env
   PRODUCT_DISPLAY_NAME=Meu Puzzle
   ```

2. Reinicie só o gateway:

   ```powershell
   cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
   docker compose up -d gateway
   ```

3. Abra o app no celular (mesma Wi‑Fi). O título vem de `GET /api/v1/product` (`Product__DisplayName` no BFF).

Identificadores técnicos (packages, realm OIDC, DynamoDB) **não** usam o nome comercial.
