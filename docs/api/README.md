# Arrow Puzzle BFF (Gateway) — contrato HTTP

## OpenAPI (arquivo estático)

[gateway-bff.openapi.yaml](gateway-bff.openapi.yaml) — espelho do contrato público `/api/v1/*` + rotas de saúde/auth dev.

## OpenAPI (runtime)

Com o Gateway no ar (Docker ou local):

| URL | Uso |
|-----|-----|
| `http://127.0.0.1:5100/openapi/v1.json` | Documento OpenAPI gerado pelo ASP.NET Core |
| `http://127.0.0.1:5100/scalar/v1` | UI Scalar para explorar e “Try it” |

Autenticação: obtenha JWT em `POST /api/v1/auth/dev-token`, depois **Authorize** com `Bearer <access_token>`.

## Postman

Coleção v3 Git-native: [../../postman/collections/arrow-puzzle-bff/](../../postman/collections/arrow-puzzle-bff/)

Environment local: [../../postman/environments/local.environment.yaml](../../postman/environments/local.environment.yaml)

Importe no Postman CLI ou Desktop; ajuste `gateway_base_url` se usar LAN (`http://192.168.x.x:5100`).

## Métricas RED (Prometheus)

Cada API ASP.NET expõe scrape em **`GET /metrics`** (Gateway `:5100`, Game Session `:5101`, etc.).

Métricas HTTP padrão do instrumentador ASP.NET Core (contadores de requests, duração, status) alimentam **Rate**, **Errors** e **Duration** por rota.
