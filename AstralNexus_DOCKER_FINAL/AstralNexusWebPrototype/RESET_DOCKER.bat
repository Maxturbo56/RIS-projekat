@echo off
setlocal
cd /d "%~dp0"
echo UPOZORENJE: Ovo brise kompletnu lokalnu Astral Nexus bazu iz Docker volume-a.
set /p confirm=Upisite DA za nastavak: 
if /I not "%confirm%"=="DA" exit /b 0
docker compose down -v
docker compose up --build -d
if errorlevel 1 (
  echo GRESKA: Reset nije uspio.
  pause
  exit /b 1
)
echo Baza je resetovana i sistem se ponovo pokrece.
pause
