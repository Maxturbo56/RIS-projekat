@echo off
setlocal
cd /d "%~dp0"
echo [Astral Nexus] Zaustavljanje Docker kontejnera...
docker compose down
pause
