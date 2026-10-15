@echo off
setlocal EnableExtensions

REM ============================================================
REM  PoliMusic - Iniciar el sitio con IIS Express
REM  El sitio queda disponible en:  http://localhost:8090/
REM
REM  Uso:
REM    iniciar-polimusic.bat          Inicia el sitio (o avisa si ya corre)
REM    iniciar-polimusic.bat stop     Detiene el sitio que este en el puerto 8090
REM ============================================================

set "IISEXPRESS=C:\Program Files\IIS Express\iisexpress.exe"
set "CONFIG=%~dp0applicationhost.config"
set "SITEURL=http://localhost:8090/"
set "PORT=8090"

REM ---------- Modo detener ----------
if /i "%~1"=="stop" goto :detener
if /i "%~1"=="parar" goto :detener

REM ---------- Comprobaciones previas ----------
if not exist "%IISEXPRESS%" (
  echo [ERROR] No se encontro IIS Express en:
  echo         %IISEXPRESS%
  echo         Instalelo desde: https://www.microsoft.com/download/details.aspx?id=48264
  echo.
  pause
  exit /b 1
)

if not exist "%CONFIG%" (
  echo [ERROR] No se encontro applicationhost.config en:
  echo         %CONFIG%
  echo.
  pause
  exit /b 1
)

REM ---------- ¿Ya esta corriendo algo en el puerto? ----------
REM  Nota: cuando IIS Express atiende el puerto, quien figura como propietario
REM  es HTTP.sys (PID 4, proceso System). Por eso aqui solo se COMPRUEBA el
REM  puerto; para detenerlo se usa el modo "stop", que si identifica iisexpress.
set "OCUPADO="
for /f "tokens=*" %%L in ('netstat -ano ^| findstr /r /c:":%PORT% " ^| findstr /i "LISTENING"') do set "OCUPADO=1"

if defined OCUPADO (
  echo.
  echo [AVISO] El puerto %PORT% ya esta en uso: PoliMusic parece estar YA EN EJECUCION.
  echo.
  echo    Direccion:  %SITEURL%
  echo    Peguela en su navegador (NO use 127.0.0.1, use "localhost").
  echo.
  echo    Cuentas verificadas:
  echo      Administrador :  userAdmin
  echo      Usuario normal:  userNormal
  echo.
  echo    Si desea reiniciarlo, ejecute primero:
  echo      iniciar-polimusic.bat stop
  echo.
  pause
  exit /b 0
)

REM ---------- Arrancar ----------
echo.
echo Iniciando PoliMusic en %SITEURL% ...
echo (Cierre esta ventana o pulse Ctrl+C para detener el sitio)
echo.

start "" "%SITEURL%"
"%IISEXPRESS%" /config:"%CONFIG%" /systray:false

endlocal
exit /b 0


REM ============================================================
REM  Subrutina: detener el sitio del puerto 8090
REM ============================================================
:detener
echo.
echo Deteniendo PoliMusic (se cerraran los procesos iisexpress.exe) ...

set "ENCONTRADO="
for /f "tokens=5" %%P in ('netstat -ano ^| findstr /r /c:":%PORT% " ^| findstr /i "LISTENING"') do (
  REM  El PID 4 es HTTP.sys (proceso System): NUNCA se debe matar.
  if not "%%P"=="0" if not "%%P"=="4" (
    taskkill /F /PID %%P >nul 2>&1
    if not errorlevel 1 (
      echo   Detenido PID %%P
      set "ENCONTRADO=1"
    )
  )
)

REM  Al cerrar IIS Express, HTTP.sys conserva el puerto unos instantes y el
REM  propietario pasa a ser el PID 4, por lo que arriba no se puede matar.
REM  Lo correcto es terminar el propio proceso de IIS Express por su nombre.
taskkill /F /IM iisexpress.exe >nul 2>&1
if not errorlevel 1 (
  echo   Detenido iisexpress.exe
  set "ENCONTRADO=1"
)

if not defined ENCONTRADO (
  echo   No habia ningun proceso de IIS Express en ejecucion.
)

echo.
echo Esperando a que se libere el puerto %PORT% ...
set /a __intentos=0
:esperar
set /a __intentos+=1
set "AUN="
for /f "tokens=*" %%L in ('netstat -ano ^| findstr /r /c:":%PORT% " ^| findstr /i "LISTENING"') do set "AUN=1"
if not defined AUN goto :puerto_libre
if %__intentos% GEQ 10 goto :puerto_libre
ping -n 2 127.0.0.1 >nul
goto :esperar

:puerto_libre
if defined AUN (
  echo   [AVISO] El puerto %PORT% sigue ocupado. Espere unos segundos y reintente.
) else (
  echo   Puerto %PORT% liberado correctamente.
)

echo.
echo Listo. Puede iniciar de nuevo con:  iniciar-polimusic.bat
echo.
pause
endlocal
exit /b 0
