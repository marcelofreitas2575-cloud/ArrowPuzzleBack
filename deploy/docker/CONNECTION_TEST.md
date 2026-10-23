# Teste de conexão (Chrome no celular)

Objetivo: **só** validar que o celular alcança o PC na porta **5100**. Sem APK, sem Keycloak.

## URL no Chrome (celular, mesma Wi‑Fi)

`http://192.168.0.38:5100/health`

Resposta esperada:

```json
{"status":"healthy","service":"gateway"}
```

Troque `192.168.0.38` se o IPv4 da PC mudar (`ipconfig` → Wi‑Fi).

## Subir o BFF para o celular alcançar (Windows)

No Docker Desktop (WSL2), a porta **5100** fica no `com.docker.backend`. O **PC** abre `http://192.168.0.38:5100`, mas o **celular** costuma ver *“Não é possível acessar esse site”*.

Use o gateway **direto no Windows** (Kestrel em `0.0.0.0:5100`):

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose down --remove-orphans

cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\services\Gateway\ArrowPuzzle.Gateway.Api
dotnet build -c Release
$env:ASPNETCORE_URLS='http://0.0.0.0:5100'
$env:Dapr__Enabled='false'
$env:Services__GameSession='http://127.0.0.1:5101'
$env:Services__LevelCatalog='http://127.0.0.1:5102'
$env:Services__PlayerProgress='http://127.0.0.1:5103'
$env:KeycloakDevAuth__Enabled='false'
$env:Authentication__Jwt__Authority='http://dev-disabled.local/realms/arrow-puzzle'
$env:Authentication__Jwt__RequireHttpsMetadata='false'
$env:Authentication__Jwt__ValidIssuers__0='http://dev-disabled.local/realms/arrow-puzzle'
dotnet run -c Release --no-build
```

Deixe esse terminal aberto. Confirme: `netstat -an | findstr ":5100"` → `0.0.0.0:5100` com processo `ArrowPuzzle.Gateway.Api` (não `com.docker.backend`).

### Alternativa Docker (só teste na PC; celular pode falhar)

Compose mínimo legado (arquivado):

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\archive
docker compose -f docker-compose.connection-test.yml up -d --build
```

Preferir stack principal com `0.0.0.0:5100` — ver [README.md](README.md).

## Parar o resto (stack completa)

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose down --remove-orphans
```

## Confirmar na PC (mesmo teste que o celular)

```powershell
curl.exe -s http://192.168.0.38:5100/health
```

Se na PC funciona e no celular não, o BFF/Docker estão OK — o bloqueio é rede PC↔celular (firewall/roteador), não o app.

## Celular: “Não é possível acessar esse site” (Wi‑Fi da casa)

Sintoma com BFF ok na PC (`curl http://192.168.0.38:5100/health` → JSON): o celular **não fala com a PC** na rede local (isolamento AP no roteador, Wi‑Fi convidado, ou celular em outra rede).

### Teste A — Hotspot da PC (contorna o roteador)

1. Windows → **Configurações** → **Rede e Internet** → **Hotspot móvel** → **Ativado**.
2. No celular, conecte no Wi‑Fi desse hotspot (desligue os dados móveis).
3. Na PC: `ipconfig` → procure IPv4 do adaptador do hotspot (geralmente **`192.168.137.1`**).
4. Chrome no celular: `http://192.168.137.1:5100/health` (troque o IP se o `ipconfig` mostrar outro).

O BFF já escuta em `0.0.0.0:5100`; funciona no hotspot sem mudar código.

### Teste B — USB + adb (sem depender da Wi‑Fi)

Com depuração USB e cabo:

```powershell
C:\Android\sdk\platform-tools\adb.exe reverse tcp:5100 tcp:5100
```

Chrome no celular: `http://127.0.0.1:5100/health`

### Conferir IP do celular na Wi‑Fi da casa

No celular (detalhes da rede Wi‑Fi), o IPv4 deve ser **`192.168.0.x`** (mesma faixa da PC `192.168.0.38`). Se for outra faixa (ex. `192.168.1.x` ou convidado), o IP `192.168.0.38` nunca vai abrir.

## Depois que `/health` voltar no celular

Stack completa (Keycloak + guest + Dapr): `docker compose up -d --build` em [deploy/docker](.) — ver [README.md](README.md).
