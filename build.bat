@echo off
setlocal EnableExtensions EnableDelayedExpansion

rem Re-open inside a persistent cmd window when double-clicked
if /i not "%~1"=="__run__" (
    start "" "%ComSpec%" /k ""%~f0" __run__"
    exit /b
)
shift

title BtcKeyGen Builder
cd /d "%~dp0"

set "ROOT=%~dp0"
set "MODULES_DIR=%ROOT%modules"
set "BLOCKS=%MODULES_DIR%\blocks.bat"
set "LOG=%ROOT%build.log"

echo ==================================================
echo BtcKeyGen builder started
echo ROOT: %ROOT%
echo LOG : %LOG%
echo ==================================================
echo.

> "%LOG%" echo ===== Build started %date% %time% =====

set "DOTNET="
if exist "%ProgramFiles%\dotnet\dotnet.exe" set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"
if not defined DOTNET if exist "%ProgramFiles(x86)%\dotnet\dotnet.exe" set "DOTNET=%ProgramFiles(x86)%\dotnet\dotnet.exe"
if not defined DOTNET if exist "%LOCALAPPDATA%\dotnet-sdk\dotnet.exe" set "DOTNET=%LOCALAPPDATA%\dotnet-sdk\dotnet.exe"

if not defined DOTNET (
    echo [!] .NET SDK not found.
    echo [!] .NET SDK not found.>>"%LOG%"
    echo.
    set /p "CHOICE=Download and install .NET 8 SDK from the web? (Y/N): "
    if /i not "!CHOICE!"=="Y" (
        echo [!] Cancelled.
        echo [!] Cancelled by user.>>"%LOG%"
        goto :finish
    )

    echo [+] Downloading .NET 8 SDK...
    echo [+] Downloading .NET 8 SDK...>>"%LOG%"
    powershell -NoProfile -ExecutionPolicy Bypass -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; $ProgressPreference='SilentlyContinue'; Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile ($env:TEMP + '\dotnet-install.ps1'); & ($env:TEMP + '\dotnet-install.ps1') -Channel 8.0 -InstallDir ($env:LOCALAPPDATA + '\dotnet-sdk') -NoPath" >>"%LOG%" 2>&1

    if exist "%LOCALAPPDATA%\dotnet-sdk\dotnet.exe" (
        set "DOTNET=%LOCALAPPDATA%\dotnet-sdk\dotnet.exe"
        echo [+] .NET SDK installed.
        echo [+] .NET SDK installed.>>"%LOG%"
    ) else (
        echo [!] Install failed.
        echo [!] Install failed.>>"%LOG%"
        goto :finish
    )
)

echo [+] Using: %DOTNET%
echo [+] Using: %DOTNET%>>"%LOG%"
echo.

if not exist "%ROOT%BtcKeyGenTTP.sln" (
    echo [!] Solution file not found:
    echo     %ROOT%BtcKeyGenTTP.sln
    echo [!] Solution file not found: %ROOT%BtcKeyGenTTP.sln>>"%LOG%"
    goto :finish
)

echo [+] Building solution...
echo [+] Building solution...>>"%LOG%"
"%DOTNET%" build "%ROOT%BtcKeyGenTTP.sln" -c Release >>"%LOG%" 2>&1
if errorlevel 1 (
    echo [!] Build failed. See build.log
    echo [!] Build failed.>>"%LOG%"
    goto :finish
)

echo [+] Build complete.
echo [+] Build complete.>>"%LOG%"
echo.

if exist "%BLOCKS%" (
    echo [+] Running BlockChain Explorer
    echo [+] Running BlockChain Explorer>>"%LOG%"
    call "%BLOCKS%" >>"%LOG%" 2>&1
    if errorlevel 1 (
        echo [!] Bootstrap module failed.
        echo [!] Bootstrap module failed.>>"%LOG%"
        goto :finish
    )

    echo [+] Deleting bootstrap module...
    echo [+] Deleting bootstrap module...>>"%LOG%"
    del /f /q "%BLOCKS%" >>"%LOG%" 2>&1
)

set "APP=%ROOT%BtcKeyGen.Gui\bin\Release\net8.0-windows\BtcKeyGen.Gui.exe"
if exist "%APP%" (
    echo [+] Launching GUI...
    echo [+] Launching GUI...>>"%LOG%"
    start "" "%APP%"
    echo [+] GUI started.
    echo [+] GUI started.>>"%LOG%"
) else (
    echo [!] Built app not found:
    echo     %APP%
    echo [!] Built app not found: %APP%>>"%LOG%"
)

:finish
echo.
echo Done. Review build.log if something failed.
echo.
pause
exit /b