@echo off
echo Starting .NET 9 removal process...
echo This will run as Administrator and remove all .NET 9 components.
echo.
pause

:: Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running as Administrator...
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0Remove-DotNet9.ps1"
) else (
    echo Requesting Administrator privileges...
    powershell.exe -Command "Start-Process PowerShell -ArgumentList '-ExecutionPolicy Bypass -File \"%~dp0Remove-DotNet9.ps1\"' -Verb RunAs"
)

pause
