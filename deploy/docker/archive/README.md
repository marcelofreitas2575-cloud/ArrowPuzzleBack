# Compose arquivados

Não use no fluxo diário. Mantidos só como referência histórica.

| Arquivo | Motivo |
|---------|--------|
| `docker-compose.connection-test.yml` | BFF mínimo só para teste LAN; substituído por stack principal + `run-gateway-lan.ps1` |
| `docker-compose.keycloak.yml` | Keycloak duplicado; IdP já está em `../docker-compose.yml`; Google bootstrap → profile `google-idp` |
| `docker-compose.dapr.yml` | Overlay vazio; Dapr integrado no compose principal |

Para subir um arquivo daqui, ajuste `cd` para esta pasta e use `-f` com os caminhos do cabeçalho de cada YAML.

Stack atual: [../README.md](../README.md).
