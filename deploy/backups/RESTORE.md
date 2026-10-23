# Restaurar backup

Backup completo (código-fonte, sem `bin`/`obj`/`.gradle`/APK build):

- **Mais recente:** `c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\backups\arrow-puzzle-full-20261022-150602.zip`
- Anterior: `c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\backups\arrow-puzzle-full-20261021-173829.zip`

## Restaurar em outra pasta

```powershell
New-Item -ItemType Directory -Force -Path c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle-restored
tar -x -f c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\backups\arrow-puzzle-full-20261022-150602.zip -C c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle-restored
```

Copie `android\local.properties` da máquina atual (não entrou no zip — gitignore).

## Docker

Compose principal inclui Keycloak + gateway. Ver `c:\Marcelo\Pessoal\Estudos\SophiaArrowPuzzle\deploy\docker\docker-compose.yml`.
