# Login com Google (Keycloak + app Android)

Sim — o **Keycloak** faz login social com Google. O app abre o navegador (Custom Tab), o Keycloak redireciona para o Google e devolve o código para `com.arrowpuzzle.app://callback`.

## 1. Google Cloud Console

1. Crie um projeto e credenciais **OAuth 2.0** do tipo **Web application**.
2. **Authorized redirect URIs** (substitua pelo host onde o Keycloak responde):
   - Celular na rede local: `http://<IP-do-PC>:8080/realms/arrow-puzzle/broker/google/endpoint`
   - Emulador Android: `http://10.0.2.2:8080/realms/arrow-puzzle/broker/google/endpoint`
   - Túnel público (recomendado se o Google recusar IP local): `https://<seu-tunel>/realms/arrow-puzzle/broker/google/endpoint`

> O Google precisa alcançar o **redirect do broker** no Keycloak. Em muitos roteadores só funciona com HTTPS + URL pública (ngrok, Cloudflare Tunnel, etc.).

## 2. Keycloak local

1. Copie `deploy/docker/.env.example` → `deploy/docker/.env` e preencha `GOOGLE_IDP_CLIENT_ID` / `GOOGLE_IDP_CLIENT_SECRET`.
2. Na raiz do repo (script) **ou** bootstrap no Compose (`--profile google-idp` — ver [deploy/docker/README.md](../docker/README.md)):
   ```powershell
   $env:GOOGLE_IDP_CLIENT_ID = "..."
   $env:GOOGLE_IDP_CLIENT_SECRET = "..."
   .\deploy\keycloak\apply-google-idp-env.ps1
   ```
3. Recrie o Keycloak para reimportar o realm:
   ```powershell
   cd deploy\docker
   docker compose up -d --force-recreate keycloak
   ```

`KC_HOSTNAME_STRICT=false` no Compose permite usar o **IP da máquina** no celular (`dev.host` no `local.properties`), sem redirecionar para `localhost`.

## 3. App Android

- `dev.host` = IP do PC na Wi‑Fi (mesmo host do Keycloak `:8080` e gateway `:5100`).
- Reinstale o APK após mudar `local.properties`.

Se o IdP não estiver configurado, o Keycloak mostra erro ao tocar em Google — e-mail, convidado e sessão salva continuam funcionando.
