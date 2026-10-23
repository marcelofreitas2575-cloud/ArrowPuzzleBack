# ADR-001: Local first e identidade com Keycloak (OIDC)

- **Status:** Aceita
- **Data:** 2026-03-22

## Contexto

O projeto **Arrow Puzzle** (nome comercial em `docs/PRODUCT_DISPLAY_NAME.md`) prioriza **ambiente local** (Docker Compose) como runtime de desenvolvimento e demonstração, sem dependência inicial de AWS, para:

- Controlar custo operacional e billing previsível durante a evolução da arquitetura.
- Separar backend containerizado (microsserviços .NET) do cliente Android (APK), integrados via rede local quando dispositivo e host compartilham o **mesmo Wi‑Fi**.
- Preservar evolução para cloud documentada (**TODO:** EC2, Terraform e **Amazon Cognito**).

Autenticação é necessária para:

- Identificar o jogador de forma estável (`sub` OIDC) no **Gateway (BFF)** e propagar identidade aos serviços internos.
- Exercitar fluxo real no app Android (Authorization Code + **PKCE**), não apenas um “userId fake” permanente.

**Amazon Cognito** não roda localmente; usá-lo desde o dia 1 obrigaria conta AWS e internet para todo teste de login.

## Decisão

1. Adotar estratégia **local first** para runtime (Docker Compose) e adiar AWS para análise documentada (ver README, seção TODO).
2. Usar **Keycloak** em container Docker como **provedor de identidade OIDC** no ambiente local, com **realm e clients versionados** no repositório (`deploy/keycloak/`), importados na subida do Compose.
3. Implementar validação de token no BFF de forma **agnóstica ao provedor**: configuração via `Authority`, `Audience` / `ValidAudiences`, issuer e JWKS — **sem** referências hardcoded a URLs Keycloak no código de domínio ou application.
4. Documentar checklist de **migração Keycloak → Cognito** nesta ADR; a troca, se aprovada no TODO AWS, será predominantemente **configuração + clients**, não reescrita da regra de negócio.

## Alternativas consideradas

| Alternativa | Prós | Contras |
|-------------|------|---------|
| **Cognito desde o início** | Um IdP em prod e dev; gerenciado AWS | Exige AWS cedo; foge do local first; dependência de conectividade externa nos testes de login |
| **JWT mock (secret fixo só dev)** | Zero container; implementação rápida | Não cobre fluxo Android/OIDC end-to-end (PKCE, refresh, JWKS) |
| **Keycloak no Docker (escolhida)** | OIDC padrão de mercado; grátis local; paridade conceitual com Cognito | +RAM (~512 MB–1 GB); configuração inicial (realm, clients) |
| **Keycloak instalado no SO** | Sem container Keycloak | Instalação manual; difere entre máquinas; pior para onboarding |

## Consequências

### Positivas

- Login e tokens **JWT OIDC** reais antes de qualquer deploy AWS.
- BFF e Android preparados para **trocar issuer** (Keycloak → Cognito) via config.
- Realm export no Git: ambiente **reproduzível** (`docker compose up`).

### Negativas / trade-offs

- Compose inclui **Postgres + Keycloak** na camada de identidade (entrega dedicada no pipeline de infra local).
- Dois conjuntos de documentação de clients (Keycloak agora; Cognito User Pool depois, se migrar).
- Usuários/senhas de **dev** no realm export — nunca usar os mesmos valores em produção.

## Migração Keycloak → Amazon Cognito (checklist)

Usar quando o TODO AWS for aprovado. Objetivo: **mesmo fluxo** (OIDC, PKCE no Android, JWT no BFF).

| Item | Keycloak (local) | Amazon Cognito |
|------|------------------|----------------|
| **Authority / Issuer** | `https://<host>/realms/arrow-puzzle` | `https://cognito-idp.<region>.amazonaws.com/<userPoolId>` |
| **JWKS** | `{Authority}/protocol/openid-connect/certs` | Mesmo padrão OIDC do pool |
| **App mobile** | Client public `arrow-puzzle-android`, PKCE, redirect URI `com.arrowpuzzle.app://callback` | App client público, PKCE, callback URLs equivalentes |
| **API / BFF** | Validação JWT Bearer; audience/client configurável | Idem; `ValidIssuer` = issuer do pool |
| **Identidade do usuário** | Claim `sub` | Claim `sub` (mapear para `userId` nos serviços) |
| **Login social (Google)** | Identity provider no realm | Google como IdP federado no User Pool |
| **Secrets** | Client secret só se client confidential (evitar secret no APK) | Mesma regra: mobile **public** + PKCE |

**Código que não deve precisar mudar:** Domain, Application handlers (desde que usem `sub` / `userId` injetado), contratos gRPC, hexagonal.

**Código/config que muda:** `appsettings`, variáveis de ambiente no Compose, redirect URIs no app Android, ADR nova ou atualização de status desta ADR para “Substituída por ADR-XXX Cognito”.

## Evolução futura

- **ADR-002:** catálogo procedural + Dapr no gateway — ver [002-procedural-catalog-and-dapr.md](002-procedural-catalog-and-dapr.md).
- **TODO AWS:** EC2, Terraform, decisão Cognito vs manter Keycloak self-hosted — **fora do escopo até análise explícita.**

## Referências

- [Keycloak — Securing applications](https://www.keycloak.org/docs/latest/securing_apps/)
- [OpenID Connect Core](https://openid.net/specs/openid-connect-core-1_0.html)
- [Amazon Cognito — OIDC identity providers](https://docs.aws.amazon.com/cognito/latest/developerguide/cognito-user-pools-oidc-idp.html)
- README do repositório: estratégia local first.
