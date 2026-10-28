# Celular não abre `http://<IP-da-PC>:5100/health`

## Comando único (como antes)

```powershell
cd c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker
docker compose down --remove-orphans
docker compose up -d --build
```

**Não** deixe `dotnet run` nem scripts de BFF no Windows rodando — ocupam a 5100 e impedem o container `gateway`.

Confirme:

```powershell
docker ps --format "{{.Names}} {{.Status}} {{.Ports}}" | findstr gateway
```

Esperado: `arrow-puzzle-gateway-1` **healthy** com `0.0.0.0:5100->5100/tcp`.

## Firewall (Administrador, uma vez)

```powershell
powershell -ExecutionPolicy Bypass -File c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\open-firewall-lan.ps1
```

## Wi‑Fi como rede Privada

```powershell
Set-NetConnectionProfile -InterfaceAlias "Wi-Fi" -NetworkCategory Private
```

(Ajuste o alias se necessário.)

## O que quebrou depois das mudanças de mTLS/Dapr

Sidecars com flags/config inválidos (Dapr 1.15) caíam → login convidado falhava mesmo com `/health` ok. O `docker-compose.yml` foi **restaurado** ao commit `7c327f2` (Dapr estável, **sem** `network_mode: service:gateway` no BFF).

## Roteador

PC abre `http://192.168.x.x:5100/health` e celular não: **isolamento AP** no roteador, ou `adb reverse` (`dev.useAdbReverse=true` no `local.properties`).
