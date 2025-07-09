# MessageBroadcast Windows Service Management (PowerShell)
# Requires Administrator privileges

#Requires -RunAsAdministrator

param(
    [Parameter(Position=0)]
    [ValidateSet('install', 'uninstall', 'start', 'stop', 'restart', 'status', 'logs', 'build', 'update')]
    [string]$Action = 'menu',
    
    [string]$ServiceName = 'MessageBroadcastServer',
    [string]$ProjectPath = '.\src\MessageBroadcast.Server',
    [string]$PublishPath = '.\publish',
    [int]$Port = 5001,
    [int]$HttpsPort = 5002
)

# Configuration
$ServiceDisplayName = 'MessageBroadcast Server'
$ServiceDescription = 'High-performance message broadcasting service using SignalR'
$ServiceExecutable = Join-Path $PublishPath 'MessageBroadcast.Server.exe'

# Colors for output
$ErrorColor = 'Red'
$SuccessColor = 'Green'
$WarningColor = 'Yellow'
$InfoColor = 'Cyan'

function Write-Header {
    param([string]$Title)
    Write-Host "`n" -NoNewline
    Write-Host "=" * 50 -ForegroundColor $InfoColor
    Write-Host $Title -ForegroundColor $InfoColor
    Write-Host "=" * 50 -ForegroundColor $InfoColor
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $SuccessColor
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $ErrorColor
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $WarningColor
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $InfoColor
}

function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)
}

function Test-DotNetSDK {
    try {
        $dotnetVersion = dotnet --version 2>$null
        if ($dotnetVersion) {
            Write-Info "Found .NET SDK version: $dotnetVersion"
            return $true
        }
    }
    catch {
        # Ignore
    }
    
    Write-Error ".NET SDK not found! Please install .NET 8 SDK."
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor $InfoColor
    return $false
}

function Get-ServiceStatus {
    try {
        $service = Get-Service -Name $ServiceName -ErrorAction Stop
        return @{
            Exists = $true
            Status = $service.Status
            StartType = (Get-WmiObject -Class Win32_Service -Filter "Name='$ServiceName'").StartMode
        }
    }
    catch {
        return @{ Exists = $false }
    }
}

function Build-Server {
    Write-Header "Building MessageBroadcast Server"
    
    if (-not (Test-DotNetSDK)) {
        return $false
    }
    
    # Clean previous publish
    if (Test-Path $PublishPath) {
        Write-Info "Cleaning previous publish..."
        Remove-Item $PublishPath -Recurse -Force
    }
    
    Write-Info "Building and publishing server..."
    Write-Host "Project Path: $ProjectPath" -ForegroundColor Gray
    Write-Host "Publish Path: $PublishPath" -ForegroundColor Gray
    
    $publishArgs = @(
        'publish', $ProjectPath,
        '-c', 'Release',
        '-o', $PublishPath,
        '--self-contained',
        '-r', 'win-x64',
        '--single-file',
        '/p:PublishSingleFile=true',
        '/p:IncludeNativeLibrariesForSelfExtract=true'
    )
    
    $result = & dotnet @publishArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build and publish completed successfully!"
        Write-Info "Published to: $PublishPath"
        return $true
    }
    else {
        Write-Error "Build failed! Exit code: $LASTEXITCODE"
        return $false
    }
}

function Install-MessageBroadcastService {
    Write-Header "Installing MessageBroadcast Service"
    
    # Check if executable exists
    if (-not (Test-Path $ServiceExecutable)) {
        Write-Error "Service executable not found!"
        Write-Warning "Please build and publish the server first."
        Write-Info "Expected location: $ServiceExecutable"
        return $false
    }
    
    # Check if service already exists
    $serviceStatus = Get-ServiceStatus
    if ($serviceStatus.Exists) {
        Write-Warning "Service '$ServiceName' already exists!"
        $response = Read-Host "Do you want to uninstall and reinstall? (y/n)"
        if ($response -eq 'y' -or $response -eq 'Y') {
            Stop-MessageBroadcastService
            Uninstall-MessageBroadcastService
        }
        else {
            return $false
        }
    }
    
    # Create the service
    Write-Info "Installing service..."
    $result = & sc create $ServiceName binPath=$ServiceExecutable DisplayName=$ServiceDisplayName start=auto depend=HTTP
    
    if ($LASTEXITCODE -eq 0) {
        # Set service description
        & sc description $ServiceName $ServiceDescription | Out-Null
        
        # Configure service recovery options
        & sc failure $ServiceName reset=86400 actions=restart/5000/restart/10000/restart/20000 | Out-Null
        
        Write-Success "Service installed successfully!"
        Write-Info "Service Name: $ServiceName"
        Write-Info "Display Name: $ServiceDisplayName"
        Write-Info "Executable: $ServiceExecutable"
        Write-Info "The service is configured to start automatically on boot."
        Write-Info "Recovery actions are configured for automatic restart on failure."
        return $true
    }
    else {
        Write-Error "Failed to install service! Exit code: $LASTEXITCODE"
        return $false
    }
}

function Start-MessageBroadcastService {
    Write-Header "Starting MessageBroadcast Service"
    
    $serviceStatus = Get-ServiceStatus
    if (-not $serviceStatus.Exists) {
        Write-Error "Service '$ServiceName' is not installed!"
        Write-Warning "Please install the service first."
        return $false
    }
    
    Write-Info "Starting service..."
    try {
        Start-Service -Name $ServiceName -ErrorAction Stop
        Write-Success "Service started successfully!"
        
        Write-Info "Waiting for service to initialize..."
        Start-Sleep -Seconds 5
        
        # Test HTTP endpoint
        Test-ServiceEndpoint
        return $true
    }
    catch {
        Write-Error "Failed to start service: $($_.Exception.Message)"
        return $false
    }
}

function Stop-MessageBroadcastService {
    Write-Header "Stopping MessageBroadcast Service"
    
    $serviceStatus = Get-ServiceStatus
    if (-not $serviceStatus.Exists) {
        Write-Warning "Service '$ServiceName' is not installed."
        return $true
    }
    
    Write-Info "Stopping service..."
    try {
        Stop-Service -Name $ServiceName -Force -ErrorAction Stop
        Write-Success "Service stopped successfully!"
        return $true
    }
    catch {
        Write-Warning "Service may already be stopped or failed to stop: $($_.Exception.Message)"
        return $true
    }
}

function Restart-MessageBroadcastService {
    Write-Header "Restarting MessageBroadcast Service"
    
    Stop-MessageBroadcastService
    Start-Sleep -Seconds 3
    Start-MessageBroadcastService
}

function Uninstall-MessageBroadcastService {
    Write-Header "Uninstalling MessageBroadcast Service"
    
    $confirmation = Read-Host "Are you sure you want to uninstall the service? (y/n)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Info "Uninstall cancelled."
        return $false
    }
    
    Stop-MessageBroadcastService
    
    Write-Info "Uninstalling service..."
    $result = & sc delete $ServiceName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Service uninstalled successfully!"
        return $true
    }
    else {
        Write-Error "Failed to uninstall service! Exit code: $LASTEXITCODE"
        return $false
    }
}

function Show-ServiceStatus {
    Write-Header "MessageBroadcast Service Status"
    
    $serviceStatus = Get-ServiceStatus
    if (-not $serviceStatus.Exists) {
        Write-Warning "Service '$ServiceName' is not installed."
        return
    }
    
    Write-Host "Service Name: " -NoNewline
    Write-Host $ServiceName -ForegroundColor $InfoColor
    
    Write-Host "Status: " -NoNewline
    $statusColor = switch ($serviceStatus.Status) {
        'Running' { $SuccessColor }
        'Stopped' { $WarningColor }
        default { $ErrorColor }
    }
    Write-Host $serviceStatus.Status -ForegroundColor $statusColor
    
    Write-Host "Start Type: " -NoNewline
    Write-Host $serviceStatus.StartType -ForegroundColor $InfoColor
    
    # Show detailed service information
    try {
        $service = Get-Service -Name $ServiceName
        $serviceDetails = Get-WmiObject -Class Win32_Service -Filter "Name='$ServiceName'"
        
        Write-Host "`nDetailed Information:" -ForegroundColor $InfoColor
        Write-Host "  Display Name: $($service.DisplayName)"
        Write-Host "  Description: $($serviceDetails.Description)"
        Write-Host "  Executable Path: $($serviceDetails.PathName)"
        Write-Host "  Process ID: $($serviceDetails.ProcessId)"
        Write-Host "  Start Mode: $($serviceDetails.StartMode)"
        Write-Host "  State: $($serviceDetails.State)"
    }
    catch {
        Write-Warning "Could not retrieve detailed service information."
    }
    
    # Test endpoint if service is running
    if ($serviceStatus.Status -eq 'Running') {
        Write-Host ""
        Test-ServiceEndpoint
    }
}

function Test-ServiceEndpoint {
    Write-Info "Testing HTTP endpoint..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$Port" -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
        Write-Success "Server is responding on http://localhost:$Port"
    }
    catch {
        Write-Error "Server is not responding on http://localhost:$Port"
        Write-Warning "The service may still be starting up or there may be a configuration issue."
    }
}

function Show-ServiceLogs {
    Write-Header "Viewing Service Logs"
    
    Write-Info "Checking Windows Event Logs for MessageBroadcast..."
    
    try {
        $events = Get-EventLog -LogName Application -Source "*MessageBroadcast*" -Newest 10 -ErrorAction SilentlyContinue
        if ($events) {
            $events | Format-Table TimeGenerated, EntryType, Message -Wrap
        }
        else {
            Write-Warning "No MessageBroadcast events found in Application log."
        }
    }
    catch {
        Write-Warning "Could not access Windows Event Log."
    }
    
    Write-Host "`nFor more detailed logs, check:" -ForegroundColor $InfoColor
    Write-Host "- Windows Event Viewer > Windows Logs > Application"
    Write-Host "- Service log files in: $PublishPath\logs\"
}

function Update-MessageBroadcastService {
    Write-Header "Updating MessageBroadcast Service"
    
    Write-Warning "This will stop the service, rebuild, and start it again."
    $confirmation = Read-Host "Continue with update? (y/n)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Info "Update cancelled."
        return
    }
    
    Write-Info "Step 1: Stopping service..."
    Stop-MessageBroadcastService
    
    Write-Info "Step 2: Building and publishing..."
    if (Build-Server) {
        Write-Info "Step 3: Starting service..."
        Start-MessageBroadcastService
        Write-Success "Service update completed!"
    }
    else {
        Write-Error "Build failed! Service remains stopped."
    }
}

function Show-Menu {
    Write-Header "MessageBroadcast Windows Service Manager"
    
    Write-Host "Select an option:" -ForegroundColor $InfoColor
    Write-Host "1. Build and Publish Server"
    Write-Host "2. Install Service"
    Write-Host "3. Start Service"
    Write-Host "4. Stop Service"
    Write-Host "5. Restart Service"
    Write-Host "6. Uninstall Service"
    Write-Host "7. View Service Status"
    Write-Host "8. View Service Logs"
    Write-Host "9. Update Service (Stop, Build, Start)"
    Write-Host "0. Exit"
    Write-Host ""
    
    do {
        $choice = Read-Host "Enter your choice (0-9)"
        switch ($choice) {
            '1' { Build-Server; break }
            '2' { Install-MessageBroadcastService; break }
            '3' { Start-MessageBroadcastService; break }
            '4' { Stop-MessageBroadcastService; break }
            '5' { Restart-MessageBroadcastService; break }
            '6' { Uninstall-MessageBroadcastService; break }
            '7' { Show-ServiceStatus; break }
            '8' { Show-ServiceLogs; break }
            '9' { Update-MessageBroadcastService; break }
            '0' { 
                Write-Header "MessageBroadcast Service Manager"
                Write-Info "Useful PowerShell commands for manual management:"
                Write-Host "- Get-Service $ServiceName                 (Check status)"
                Write-Host "- Start-Service $ServiceName               (Start service)"
                Write-Host "- Stop-Service $ServiceName                (Stop service)"
                Write-Host "- Set-Service $ServiceName -StartupType Automatic  (Enable auto-start)"
                Write-Host "- Set-Service $ServiceName -StartupType Manual     (Disable auto-start)"
                Write-Host ""
                Write-Info "Service will be available at:"
                Write-Host "- HTTP:  http://localhost:$Port"
                Write-Host "- HTTPS: https://localhost:$HttpsPort"
                Write-Host ""
                Write-Success "Thank you for using MessageBroadcast! 🚀"
                return
            }
            default { 
                Write-Warning "Invalid choice. Please try again."
            }
        }
        
        if ($choice -ne '0') {
            Write-Host ""
            Read-Host "Press Enter to continue"
            Show-Menu
        }
    } while ($choice -ne '0')
}

# Main execution
if (-not (Test-Administrator)) {
    Write-Error "This script requires Administrator privileges!"
    Write-Warning "Please run PowerShell as Administrator."
    exit 1
}

# Execute based on parameter
switch ($Action) {
    'build' { Build-Server }
    'install' { Install-MessageBroadcastService }
    'start' { Start-MessageBroadcastService }
    'stop' { Stop-MessageBroadcastService }
    'restart' { Restart-MessageBroadcastService }
    'uninstall' { Uninstall-MessageBroadcastService }
    'status' { Show-ServiceStatus }
    'logs' { Show-ServiceLogs }
    'update' { Update-MessageBroadcastService }
    'menu' { Show-Menu }
    default { Show-Menu }
}
