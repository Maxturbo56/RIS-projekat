@echo off
setlocal
cd /d "%~dp0"
echo [Astral Nexus] Pokretanje aplikacije i SQL Server baze...
docker compose up --build -d
if errorlevel 1 (
  echo.
  echo GRESKA: Docker Compose nije uspio. Provjerite da li je Docker Desktop pokrenut.
  pause
  exit /b 1
)
echo.
echo Kontejneri su pokrenuti.
echo Otvorite http://localhost:5092 nakon sto se SQL Server inicijalizira.
echo Za pregled stanja: docker compose ps
pause
