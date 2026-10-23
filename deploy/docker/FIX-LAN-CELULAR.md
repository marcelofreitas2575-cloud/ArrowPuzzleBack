# Celular não abre `http://192.168.0.38:5100/health`

## O que quebrou no projeto (já corrigido no repo)

- Sidecar **`gateway-dapr`** com `network_mode: service:gateway` — a porta **5100** deixava de ser publicada direito. Correção: sidecar na rede do Compose com `-app-channel-address gateway` (ver `docker-compose.yml`).
- Experimento de portas **8080/8090** — revertido; gateway na **5100**.

## O que a PC mostra vs o celular

- Na **PC**, `http://192.168.0.38:5100/health` pode funcionar **mesmo quando o celular não alcança** (loopback / hairpin no Windows).
- O Docker Desktop (WSL2) atende a LAN via **`com.docker.backend.exe`**. Sem regra de firewall correta, só o host local passa.

Documentação: [Docker Desktop networking](https://docs.docker.com/desktop/features/networking/) — *“Host firewalls can permit or deny inbound connections by filtering on com.docker.backend.”*

## Correção (uma vez, como Administrador)

1. Subir o gateway:

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose up -d gateway
```

2. Firewall + Wi‑Fi **Privado** (não deixe FamiliaCarvalho como **Público** para dev):

```powershell
powershell -ExecutionPolicy Bypass -File c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\open-firewall-lan.ps1
```

*(Clique direito no PowerShell → **Executar como administrador**, depois rode o comando acima.)*

3. Chrome no celular (mesma Wi‑Fi, IPv4 do celular `192.168.0.x`):

`http://192.168.0.38:5100/health`

## Docker Desktop (conferir)

Settings → **Network** → **Port binding behavior** = **Default port binding** (não “Local only”).  
No seu Desktop 4.78 o analytics já indica `default-port-binding`.

## Se ainda falhar depois do script admin

Isolamento **AP / cliente‑cliente** no roteador (comum em mesh). Não é código do jogo — o roteador impede celular → PC. Opções: desativar isolamento AP na administração do Wi‑Fi ou usar depuração USB + `adb reverse tcp:5100 tcp:5100` e `http://127.0.0.1:5100/health` no Chrome.
