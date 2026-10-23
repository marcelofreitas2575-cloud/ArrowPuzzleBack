# Login com Google — o que **você** faz (5 min) e o que o **Docker** faz sozinho

Eu não consigo criar credenciais na **sua** conta Google por você (só você tem acesso ao Google Cloud).  
Depois que você colar **duas linhas** no arquivo `.env`, o script `keycloak-google-bootstrap` configura o Keycloak automaticamente.

---

## Passo 1 — Google Cloud (obrigatório, uma vez)

1. Abra [Google Cloud Console → Credentials](https://console.cloud.google.com/apis/credentials).
2. Crie um projeto (se ainda não tiver).
3. **OAuth consent screen** → External → preencha nome do app → salve (escopo `email`, `profile`, `openid` já bastam).
4. **Create credentials** → **OAuth client ID** → tipo **Web application**.
5. Em **Authorized redirect URIs**, adicione **exatamente** (troque o host se seu IP for outro):

   ```
   http://192.168.0.38:8080/realms/arrow-puzzle/broker/google/endpoint
   ```

   > **Celular na Wi‑Fi:** o Google precisa chamar esse URL **dos servidores deles**. IP `192.168.x.x` **muitas vezes não funciona**.  
   > Se der erro no Google, use **ngrok** (Passo 1b abaixo).

6. Copie o **Client ID** e o **Client secret**.

### Passo 1b — Se o Google recusar IP local (comum no celular)

1. Instale [ngrok](https://ngrok.com/) e rode: `ngrok http 8080`
2. Use a URL HTTPS que aparecer, por exemplo `https://abc123.ngrok-free.app`
3. Redirect URI no Google:

   ```
   https://abc123.ngrok-free.app/realms/arrow-puzzle/broker/google/endpoint
   ```

4. No `.env` (Passo 2), defina também:
   - `KEYCLOAK_PUBLIC_BASE_URL=https://abc123.ngrok-free.app`
   - `KEYCLOAK_PUBLIC_REDIRECT_URI=https://abc123.ngrok-free.app/realms/arrow-puzzle/broker/google/endpoint`
5. No `android/local.properties`:
   - `dev.host=192.168.0.38` (gateway continua no IP da PC)
   - `dev.keycloak.publicUrl=https://abc123.ngrok-free.app` (só login abre pelo túnel)

Recompile o APK (`assembleDebug`) depois de mudar `local.properties`.

---

## Passo 2 — Arquivo `.env` (colar credenciais)

Na pasta `deploy/docker`:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
copy .env.example .env
notepad .env
```

Preencha:

```env
GOOGLE_IDP_CLIENT_ID=xxxx.apps.googleusercontent.com
GOOGLE_IDP_CLIENT_SECRET=GOCSPX-xxxx
KEYCLOAK_FRONTEND_URL=http://192.168.0.38:8080
KEYCLOAK_PUBLIC_REDIRECT_URI=http://192.168.0.38:8080/realms/arrow-puzzle/broker/google/endpoint
```

(Com ngrok, use as URLs HTTPS do Passo 1b.)

---

## Passo 3 — Subir / aplicar no Docker (automático)

Com o stack já rodando ou não:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose up -d keycloak
docker compose run --rm keycloak-google-bootstrap
```

Não precisa apagar volumes. Só o serviço **bootstrap** fala com o Keycloak e liga o Google.

Confirme no log: `Google habilitado`.

---

## Passo 4 — App

- `android/local.properties` → `dev.host` = IP da PC (mesma rede Wi‑Fi).
- Reinstale o APK se mudou `dev.keycloak.publicUrl`.
- Na tela de login → **Continuar com Google**.

---

## Resumo

| Quem | O quê |
|------|--------|
| **Você** | Client ID + Secret no Google Cloud + `.env` |
| **Repositório / Docker** | `bootstrap-google-idp.sh` habilita o IdP Google no Keycloak |

Sem Client ID/Secret no `.env`, o Google **não** funciona — não há como contornar isso.
