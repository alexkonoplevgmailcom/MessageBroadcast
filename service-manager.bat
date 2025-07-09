@echo off
setlocal EnableDelayedExpansion

:: MessageBroadcast Windows Service Management Script
:: Requires Administrator privileges for service operations

echo.
echo ===============================================
echo MessageBroadcast Windows Service Manager
echo ===============================================
echo.

:: Check if running as administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script requires Administrator privileges!
    echo Please run as Administrator.
    echo.
    pause
    exit /b 1
)

:: Configuration
set SERVICE_NAME=MessageBroadcastServer
set SERVICE_DISPLAY_NAME=MessageBroadcast Server
set SERVICE_DESCRIPTION=High-performance message broadcasting service using SignalR
set SERVICE_EXECUTABLE=%~dp0publish\MessageBroadcast.Server.exe
set PROJECT_PATH=%~dp0src\MessageBroadcast.Server
set PUBLISH_PATH=%~dp0publish

:: Show menu
:menu
echo.
echo Select an option:
echo 1. Build and Publish Server
echo 2. Install Service
echo 3. Start Service
echo 4. Stop Service
echo 5. Restart Service
echo 6. Uninstall Service
echo 7. View Service Status
echo 8. View Service Logs
echo 9. Update Service (Stop, Build, Start)
echo 0. Exit
echo.
set /p choice="Enter your choice (0-9): "

if "%choice%"=="1" goto :build
if "%choice%"=="2" goto :install
if "%choice%"=="3" goto :start
if "%choice%"=="4" goto :stop
if "%choice%"=="5" goto :restart
if "%choice%"=="6" goto :uninstall
if "%choice%"=="7" goto :status
if "%choice%"=="8" goto :logs
if "%choice%"=="9" goto :update
if "%choice%"=="0" goto :exit
echo Invalid choice. Please try again.
goto :menu

:build
echo.
echo ==========================================
echo Building and Publishing MessageBroadcast Server
echo ==========================================
echo.

:: Check if .NET SDK is installed
dotnet --version >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: .NET SDK not found! Please install .NET 8 SDK.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    goto :menu
)

:: Clean previous publish
if exist "%PUBLISH_PATH%" (
    echo Cleaning previous publish...
    rmdir /s /q "%PUBLISH_PATH%"
)

:: Build and publish
echo Building and publishing server...
dotnet publish "%PROJECT_PATH%" -c Release -o "%PUBLISH_PATH%" --self-contained -r win-x64 --single-file /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

if %errorLevel% equ 0 (
    echo.
    echo ✅ Build and publish completed successfully!
    echo Published to: %PUBLISH_PATH%
) else (
    echo.
    echo ❌ Build failed! Please check the output above.
)
echo.
pause
goto :menu

:install
echo.
echo ==========================================
echo Installing MessageBroadcast Service
echo ==========================================
echo.

:: Check if executable exists
if not exist "%SERVICE_EXECUTABLE%" (
    echo ERROR: Service executable not found!
    echo Please build and publish the server first (option 1).
    echo Expected location: %SERVICE_EXECUTABLE%
    pause
    goto :menu
)

:: Check if service already exists
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorLevel% equ 0 (
    echo WARNING: Service '%SERVICE_NAME%' already exists!
    set /p confirm="Do you want to uninstall and reinstall? (y/n): "
    if /i "!confirm!"=="y" (
        call :stop_service
        call :uninstall_service
    ) else (
        goto :menu
    )
)

:: Create the service
echo Installing service...
sc create "%SERVICE_NAME%" binPath="%SERVICE_EXECUTABLE%" DisplayName="%SERVICE_DISPLAY_NAME%" start=auto depend=HTTP

if %errorLevel% equ 0 (
    :: Set service description
    sc description "%SERVICE_NAME%" "%SERVICE_DESCRIPTION%"
    
    :: Configure service recovery options
    sc failure "%SERVICE_NAME%" reset=86400 actions=restart/5000/restart/10000/restart/20000
    
    echo.
    echo ✅ Service installed successfully!
    echo Service Name: %SERVICE_NAME%
    echo Display Name: %SERVICE_DISPLAY_NAME%
    echo Executable: %SERVICE_EXECUTABLE%
    echo.
    echo The service is configured to start automatically on boot.
    echo Recovery actions are configured for automatic restart on failure.
) else (
    echo.
    echo ❌ Failed to install service!
)
echo.
pause
goto :menu

:start
echo.
echo ==========================================
echo Starting MessageBroadcast Service
echo ==========================================
echo.
call :start_service
pause
goto :menu

:stop
echo.
echo ==========================================
echo Stopping MessageBroadcast Service
echo ==========================================
echo.
call :stop_service
pause
goto :menu

:restart
echo.
echo ==========================================
echo Restarting MessageBroadcast Service
echo ==========================================
echo.
call :stop_service
timeout /t 3 /nobreak >nul
call :start_service
pause
goto :menu

:uninstall
echo.
echo ==========================================
echo Uninstalling MessageBroadcast Service
echo ==========================================
echo.
set /p confirm="Are you sure you want to uninstall the service? (y/n): "
if /i "%confirm%" neq "y" goto :menu

call :stop_service
call :uninstall_service
pause
goto :menu

:status
echo.
echo ==========================================
echo MessageBroadcast Service Status
echo ==========================================
echo.
sc query "%SERVICE_NAME%"
echo.

:: Check if service is responding on HTTP
echo Checking HTTP endpoint...
powershell -Command "try { $response = Invoke-WebRequest -Uri 'http://localhost:5001' -TimeoutSec 5 -UseBasicParsing; Write-Host '✅ Server is responding on http://localhost:5001' -ForegroundColor Green } catch { Write-Host '❌ Server is not responding on http://localhost:5001' -ForegroundColor Red }"

echo.
pause
goto :menu

:logs
echo.
echo ==========================================
echo Viewing Service Logs
echo ==========================================
echo.
echo Checking Windows Event Logs for MessageBroadcast...
echo.

:: Show recent application events
powershell -Command "Get-EventLog -LogName Application -Source 'MessageBroadcast*' -Newest 10 -ErrorAction SilentlyContinue | Format-Table TimeGenerated, EntryType, Message -Wrap"

echo.
echo For more detailed logs, check:
echo - Windows Event Viewer ^> Windows Logs ^> Application
echo - Service log files in: %PUBLISH_PATH%\logs\
echo.
pause
goto :menu

:update
echo.
echo ==========================================
echo Updating MessageBroadcast Service
echo ==========================================
echo.
echo This will stop the service, rebuild, and start it again.
set /p confirm="Continue with update? (y/n): "
if /i "%confirm%" neq "y" goto :menu

echo.
echo Step 1: Stopping service...
call :stop_service

echo.
echo Step 2: Building and publishing...
goto :build_for_update

:build_for_update
:: Clean previous publish
if exist "%PUBLISH_PATH%" (
    echo Cleaning previous publish...
    rmdir /s /q "%PUBLISH_PATH%"
)

:: Build and publish
echo Building and publishing server...
dotnet publish "%PROJECT_PATH%" -c Release -o "%PUBLISH_PATH%" --self-contained -r win-x64 --single-file /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

if %errorLevel% equ 0 (
    echo.
    echo ✅ Build completed successfully!
    echo.
    echo Step 3: Starting service...
    call :start_service
    echo.
    echo ✅ Service update completed!
) else (
    echo.
    echo ❌ Build failed! Service remains stopped.
)
echo.
pause
goto :menu

:: Helper functions
:start_service
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Service '%SERVICE_NAME%' is not installed!
    echo Please install the service first (option 2).
    exit /b 1
)

echo Starting service...
sc start "%SERVICE_NAME%"
if %errorLevel% equ 0 (
    echo ✅ Service started successfully!
    echo.
    echo Waiting for service to initialize...
    timeout /t 5 /nobreak >nul
    
    :: Test HTTP endpoint
    powershell -Command "try { $response = Invoke-WebRequest -Uri 'http://localhost:5001' -TimeoutSec 10 -UseBasicParsing; Write-Host '✅ Server is running and responding on http://localhost:5001' -ForegroundColor Green } catch { Write-Host '⚠️  Service started but may still be initializing...' -ForegroundColor Yellow }"
) else (
    echo ❌ Failed to start service!
)
exit /b %errorLevel%

:stop_service
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorLevel% neq 0 (
    echo Service '%SERVICE_NAME%' is not installed.
    exit /b 0
)

echo Stopping service...
sc stop "%SERVICE_NAME%"
if %errorLevel% equ 0 (
    echo ✅ Service stopped successfully!
) else (
    echo ⚠️  Service may already be stopped or failed to stop.
)
exit /b 0

:uninstall_service
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorLevel% neq 0 (
    echo Service '%SERVICE_NAME%' is not installed.
    exit /b 0
)

echo Uninstalling service...
sc delete "%SERVICE_NAME%"
if %errorLevel% equ 0 (
    echo ✅ Service uninstalled successfully!
) else (
    echo ❌ Failed to uninstall service!
)
exit /b %errorLevel%

:exit
echo.
echo ==========================================
echo MessageBroadcast Service Manager
echo ==========================================
echo.
echo Useful commands for manual management:
echo - sc query %SERVICE_NAME%           (Check status)
echo - sc start %SERVICE_NAME%           (Start service)
echo - sc stop %SERVICE_NAME%            (Stop service)
echo - sc config %SERVICE_NAME% start=auto  (Enable auto-start)
echo - sc config %SERVICE_NAME% start=demand (Disable auto-start)
echo.
echo Service will be available at:
echo - HTTP:  http://localhost:5001
echo - HTTPS: https://localhost:5002
echo.
echo Thank you for using MessageBroadcast! 🚀
echo.
pause
exit /b 0
