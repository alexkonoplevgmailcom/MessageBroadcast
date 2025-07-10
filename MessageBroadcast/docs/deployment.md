# MessageBroadcast Deployment Guide

## 📋 Overview
This comprehensive guide covers deploying the MessageBroadcast server across multiple platforms and deployment scenarios, with special focus on Windows Service automation and production-ready configurations.

## 🔧 Prerequisites

### System Requirements
- **.NET 8 Runtime** (for running pre-built binaries) or **.NET 8 SDK** (for building from source)
- **Memory**: Minimum 512MB RAM (recommended 1GB+ for high throughput)
- **Disk Space**: 100MB for application files and logs
- **Network**: Local network access (no internet connectivity required)
- **Ports**: Default HTTP 5001, HTTPS 5002 (configurable)

### Development Requirements (Building from Source)
- **.NET 8 SDK** (latest stable version)
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Git**: For source code management

## 🏗️ Building the Application

### Quick Build Commands
```bash
# Clone or navigate to source directory
cd MessageBroadcast

# Restore all NuGet dependencies
dotnet restore

# Build entire solution in Release mode
dotnet build --configuration Release

# Run unit tests to verify build
dotnet test
```

### Production Builds

**Self-contained single-file deployments** (recommended for production):

```bash
# Windows x64 (includes .NET runtime)
dotnet publish src/MessageBroadcast.Server -c Release -o ./publish/win-x64 \
  --self-contained -r win-x64 --single-file \
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

# Windows ARM64 
dotnet publish src/MessageBroadcast.Server -c Release -o ./publish/win-arm64 \
  --self-contained -r win-arm64 --single-file

# macOS x64 (Intel)
dotnet publish src/MessageBroadcast.Server -c Release -o ./publish/osx-x64 \
  --self-contained -r osx-x64 --single-file

# macOS ARM64 (Apple Silicon M1/M2)
dotnet publish src/MessageBroadcast.Server -c Release -o ./publish/osx-arm64 \
  --self-contained -r osx-arm64 --single-file

# Linux x64
dotnet publish src/MessageBroadcast.Server -c Release -o ./publish/linux-x64 \
  --self-contained -r linux-x64 --single-file
```

## 🚀 Deployment Options

### Option 1: Self-Contained Deployment (Recommended)
Self-contained deployments include the .NET runtime and can run without .NET installed on the target machine.

**Framework-dependent deployment** (requires .NET 9 runtime on target):
```bash
dotnet publish src/MessageBroadcast.Server -c Release -o ./publish/portable --no-self-contained
```

## 🪟 Windows Service Deployment

MessageBroadcast includes **automated Windows Service management scripts** for production deployment.

### Automated Service Management

**Option A: Interactive Batch Script**
```cmd
# Run as Administrator
service-manager.bat
```

**Option B: PowerShell Command Line**
```powershell
# Install and start service
.\service-manager.ps1 -Action install
.\service-manager.ps1 -Action start

# Or use interactive menu
.\service-manager.ps1
```

### Service Management Operations

| Operation | Batch Script | PowerShell Script |
|-----------|--------------|------------------|
| **Build & Publish** | Menu Option 1 | `.\service-manager.ps1 build` |
| **Install Service** | Menu Option 2 | `.\service-manager.ps1 install` |
| **Start Service** | Menu Option 3 | `.\service-manager.ps1 start` |
| **Stop Service** | Menu Option 4 | `.\service-manager.ps1 stop` |
| **Restart Service** | Menu Option 5 | `.\service-manager.ps1 restart` |
| **Uninstall Service** | Menu Option 6 | `.\service-manager.ps1 uninstall` |
| **View Status** | Menu Option 7 | `.\service-manager.ps1 status` |
| **View Logs** | Menu Option 8 | `.\service-manager.ps1 logs` |
| **Update Service** | Menu Option 9 | `.\service-manager.ps1 update` |

### Service Configuration Details

**Service Properties:**
- **Service Name**: `MessageBroadcastServer`
- **Display Name**: `MessageBroadcast Server`
- **Startup Type**: Automatic (starts on boot)
- **Dependencies**: HTTP service
- **Recovery Action**: Auto-restart on failure
- **User Account**: Local System (configurable)

**Default Network Configuration:**
- **HTTP Endpoint**: `http://localhost:5001`
- **HTTPS Endpoint**: `https://localhost:5002`
- **Health Check**: `GET /health`

### Service Installation Prerequisites

1. **Administrator Privileges**: Required for service installation/management
2. **Windows Service Support**: Windows 7/Server 2008 R2 or later
3. **Network Configuration**: Ensure ports 5001/5002 are available
4. **Firewall Rules**: May need to configure for network access

### Manual Service Commands

For advanced users, these Windows `sc` commands provide direct service control:

```cmd
# Check service status
sc query MessageBroadcastServer

# Start/stop service manually
sc start MessageBroadcastServer
sc stop MessageBroadcastServer

# Configure auto-start
sc config MessageBroadcastServer start=auto

# Configure recovery options
sc failure MessageBroadcastServer reset=86400 actions=restart/5000/restart/10000/restart/20000
```

## ⚙️ Configuration

### Server Configuration
The primary configuration file is `appsettings.json` in the deployment directory:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.SignalR": "Debug"
    }
  },
  "Server": {
    "Port": 5001,
    "HttpsPort": 5002,
    "EnableDetailedErrors": false,
    "KeepAliveIntervalSeconds": 15,
    "ClientTimeoutIntervalSeconds": 30,
    "MaxConcurrentConnections": 1000,
    "LogConnections": true
  },
  "AllowedHosts": "*"
}
```

### Environment Variables

Override configuration using environment variables:

```bash
# Server ports
export ASPNETCORE_URLS="http://localhost:8080;https://localhost:8443"

# Environment mode
export ASPNETCORE_ENVIRONMENT=Production

# Custom server settings
export Server__Port=8080
export Server__MaxConcurrentConnections=2000
export Server__KeepAliveIntervalSeconds=30
```

### Advanced Configuration

**Performance Tuning:**
```json
{
  "Server": {
    "MaxConcurrentConnections": 5000,
    "KeepAliveIntervalSeconds": 30,
    "ClientTimeoutIntervalSeconds": 60,
    "EnableDetailedErrors": false
  },
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 5000,
      "MaxConcurrentUpgradedConnections": 5000
    }
  }
}
```

### Firewall Configuration
For local network access, ensure the configured port (default: 5000) is open:

**Windows:**
```cmd
netsh advfirewall firewall add rule name="MessageBroadcast Server" dir=in action=allow protocol=TCP localport=5000
```

**macOS:**
```bash
# macOS firewall typically allows localhost connections by default
# For network access, configure through System Preferences > Security & Privacy > Firewall
```

## Running the Server

### Windows
```cmd
# Navigate to deployment folder
cd deploy\windows

# Run the server
MessageBroadcast.Server.exe
```

### macOS
```bash
# Navigate to deployment folder
cd deploy/macos

# Make executable (if needed)
chmod +x MessageBroadcast.Server

# Run the server
./MessageBroadcast.Server
```

### Verification
The server should display:
```
MessageBroadcast Server starting on port 5000
Press Ctrl+C to shutdown
```

Test the health endpoint:
```bash
curl http://localhost:5000/health
# Should return: OK
```

## Running Sample Applications

### Publisher Sample
```bash
# Build and run publisher
dotnet run --project samples/Publisher.Sample/Publisher.Sample.csproj

# Or from published binary
./Publisher.Sample
```

### Subscriber Sample
```bash
# Build and run subscriber
dotnet run --project samples/Subscriber.Sample/Subscriber.Sample.csproj

# Or from published binary
./Subscriber.Sample
```

## Performance Tuning

### High Throughput Configuration
For handling hundreds of messages per second:

1. **Increase connection limits** in `appsettings.json`:
```json
{
  "Server": {
    "MaxConcurrentConnections": 5000,
    "KeepAliveIntervalSeconds": 30,
    "ClientTimeoutIntervalSeconds": 60
  }
}
```

2. **System-level optimizations:**
   - Increase TCP connection limits
   - Optimize garbage collection settings
   - Use dedicated network interfaces

3. **Environment variables for .NET optimization:**
```bash
export DOTNET_gcServer=1
export DOTNET_gcConcurrent=1
export ASPNETCORE_ENVIRONMENT=Production
```

## Monitoring and Troubleshooting

### Built-in Monitoring
- Connection count: Available through the ConnectionManager
- Health check endpoint: `GET /health`
- Console logging for connection events

### Common Issues

**Port Already in Use:**
```bash
# Windows
netstat -an | findstr :5000

# macOS/Linux
lsof -i :5000
```

**High Memory Usage:**
- Monitor connection count
- Check for connection leaks
- Adjust GC settings if needed

**Network Connectivity:**
- Verify firewall settings
- Check network interface binding
- Test with telnet: `telnet localhost 5000`

## Security Considerations

### Standalone Environment Security
- No authentication required for standalone operation
- Local network isolation recommended
- Consider VPN for multi-machine deployments

### Production Considerations
- Implement connection limits
- Add rate limiting if needed
- Monitor resource usage
- Use dedicated service accounts

## Service Installation

### Windows Service
Use tools like `NSSM` or create a Windows Service wrapper for automatic startup.

### macOS LaunchDaemon
Create a `.plist` file for automatic startup with launchd.

### Docker Container
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY ./publish/server /app
WORKDIR /app
EXPOSE 5000
ENTRYPOINT ["./MessageBroadcast.Server"]
```

## Backup and Maintenance

### Application Updates
1. Stop the server
2. Replace binaries
3. Update configuration if needed
4. Restart the server

### No persistent data - no backup required for message content
The server operates in fire-and-forget mode with no persistent storage.

## 🖥️ Cross-Platform Manual Deployment

### Windows Manual Deployment
```cmd
# Navigate to published application
cd publish\win-x64

# Run server directly
MessageBroadcast.Server.exe

# Or with custom configuration
MessageBroadcast.Server.exe --urls "http://localhost:8080"
```

### macOS Manual Deployment
```bash
# Navigate to published application
cd publish/osx-x64

# Make executable (if needed)
chmod +x MessageBroadcast.Server

# Run server
./MessageBroadcast.Server

# Or with custom configuration
./MessageBroadcast.Server --urls "http://localhost:8080"
```

### Linux Manual Deployment
```bash
# Navigate to published application  
cd publish/linux-x64

# Make executable
chmod +x MessageBroadcast.Server

# Run server
./MessageBroadcast.Server

# Run as background service (systemd example)
sudo systemctl start messagebroadcast
```

## 🐳 Docker Deployment

### Dockerfile
```dockerfile
# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Set working directory
WORKDIR /app

# Copy published application
COPY ./publish/linux-x64 .

# Expose ports
EXPOSE 5001 5002

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5001;https://+:5002

# Create non-root user for security
RUN adduser --disabled-password --gecos "" messagebroadcast
USER messagebroadcast

# Start the application
ENTRYPOINT ["./MessageBroadcast.Server"]
```

### Docker Build and Run
```bash
# Build the Docker image
docker build -t messagebroadcast:latest .

# Run the container
docker run -d \
  --name messagebroadcast-server \
  -p 5001:5001 \
  -p 5002:5002 \
  --restart unless-stopped \
  messagebroadcast:latest

# Check container status
docker ps
docker logs messagebroadcast-server
```

### Docker Compose
```yaml
version: '3.8'

services:
  messagebroadcast:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5001:5001"
      - "5002:5002"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Server__MaxConcurrentConnections=2000
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5001/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

## 🔧 Linux System Service Setup

### Systemd Service Configuration

Create a systemd service file for automatic startup on Linux:

**`/etc/systemd/system/messagebroadcast.service`**
```ini
[Unit]
Description=MessageBroadcast High-Performance Message Broadcasting Server
Documentation=https://github.com/your-username/MessageBroadcast
After=network.target
Wants=network.target

[Service]
Type=notify
# Path to the published executable
ExecStart=/opt/messagebroadcast/MessageBroadcast.Server
# Working directory
WorkingDirectory=/opt/messagebroadcast
# Run as dedicated user
User=messagebroadcast
Group=messagebroadcast
# Restart policy
Restart=always
RestartSec=5
# Environment variables
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5001;https://localhost:5002
# Security settings
NoNewPrivileges=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/opt/messagebroadcast/logs

[Install]
WantedBy=multi-user.target
```

### Linux Service Management Commands
```bash
# Create dedicated user
sudo useradd --system --shell /usr/sbin/nologin messagebroadcast

# Copy application files
sudo mkdir -p /opt/messagebroadcast
sudo cp -r ./publish/linux-x64/* /opt/messagebroadcast/
sudo chown -R messagebroadcast:messagebroadcast /opt/messagebroadcast
sudo chmod +x /opt/messagebroadcast/MessageBroadcast.Server

# Install and start service
sudo systemctl daemon-reload
sudo systemctl enable messagebroadcast
sudo systemctl start messagebroadcast

# Check service status
sudo systemctl status messagebroadcast
sudo journalctl -u messagebroadcast -f
```

## 🍎 macOS LaunchDaemon Setup

### LaunchDaemon Configuration

Create a launch daemon for automatic startup on macOS:

**`/Library/LaunchDaemons/com.messagebroadcast.server.plist`**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" 
  "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.messagebroadcast.server</string>
    
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/messagebroadcast/MessageBroadcast.Server</string>
    </array>
    
    <key>WorkingDirectory</key>
    <string>/usr/local/bin/messagebroadcast</string>
    
    <key>UserName</key>
    <string>messagebroadcast</string>
    
    <key>KeepAlive</key>
    <true/>
    
    <key>RunAtLoad</key>
    <true/>
    
    <key>EnvironmentVariables</key>
    <dict>
        <key>ASPNETCORE_ENVIRONMENT</key>
        <string>Production</string>
        <key>ASPNETCORE_URLS</key>
        <string>http://localhost:5001;https://localhost:5002</string>
    </dict>
    
    <key>StandardOutPath</key>
    <string>/var/log/messagebroadcast/stdout.log</string>
    <key>StandardErrorPath</key>
    <string>/var/log/messagebroadcast/stderr.log</string>
</dict>
</plist>
```

### macOS Service Management Commands
```bash
# Create dedicated user
sudo dscl . -create /Users/messagebroadcast
sudo dscl . -create /Users/messagebroadcast UserShell /usr/bin/false
sudo dscl . -create /Users/messagebroadcast RealName "MessageBroadcast Server"

# Copy application files
sudo mkdir -p /usr/local/bin/messagebroadcast
sudo cp -r ./publish/osx-*/‌* /usr/local/bin/messagebroadcast/
sudo chown -R messagebroadcast:staff /usr/local/bin/messagebroadcast
sudo chmod +x /usr/local/bin/messagebroadcast/MessageBroadcast.Server

# Create log directory
sudo mkdir -p /var/log/messagebroadcast
sudo chown messagebroadcast:staff /var/log/messagebroadcast

# Load and start service
sudo launchctl load /Library/LaunchDaemons/com.messagebroadcast.server.plist
sudo launchctl start com.messagebroadcast.server

# Check service status
sudo launchctl list | grep messagebroadcast
```

## 📊 Production Monitoring and Maintenance

### Health Check Integration
The server provides built-in health check endpoints:

```bash
# Check server health
curl http://localhost:5001/health
# Expected response: "OK" with HTTP 200

# Check from external monitoring tools
wget --spider --server-response http://localhost:5001/health 2>&1 | grep "200 OK"
```

### Log Management
Configure log rotation and monitoring:

**Windows Event Logs:**
- Application events logged to Windows Event Log
- Use Event Viewer to monitor service events
- Filter by source: "MessageBroadcast"

**Linux/macOS Logs:**
```bash
# View real-time logs
sudo journalctl -u messagebroadcast -f

# View logs with filtering
sudo journalctl -u messagebroadcast --since "1 hour ago"

# Log rotation with logrotate (/etc/logrotate.d/messagebroadcast)
/var/log/messagebroadcast/*.log {
    daily
    rotate 30
    compress
    delaycompress
    copytruncate
    notifempty
    missingok
}
```

### Performance Monitoring

**Key Metrics to Monitor:**
- **Connection Count**: Number of active SignalR connections
- **Message Throughput**: Messages per second
- **Memory Usage**: Should remain stable under normal load
- **CPU Usage**: Typically <25% under high load
- **Response Time**: Health check endpoint latency

**Monitoring Tools Integration:**
```bash
# Prometheus metrics (if implemented)
curl http://localhost:5001/metrics

# Custom monitoring script
#!/bin/bash
HEALTH_CHECK=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5001/health)
if [ $HEALTH_CHECK -eq 200 ]; then
    echo "MessageBroadcast: OK"
else
    echo "MessageBroadcast: FAILED (HTTP $HEALTH_CHECK)"
    # Send alert
fi
```

## 🔒 Security Considerations

### Network Security
- **Default Configuration**: Binds to localhost only
- **Network Access**: Configure CORS and allowed hosts for network access
- **HTTPS**: Enable HTTPS for production deployments
- **Firewall**: Restrict access to necessary ports only

### Application Security
```json
{
  "Server": {
    "EnableDetailedErrors": false,
    "MaxConcurrentConnections": 1000,
    "EnableCorsForDevelopment": false
  },
  "AllowedHosts": "localhost;your-domain.com"
}
```

### Service Account Security
- **Windows**: Use dedicated service account with minimal privileges
- **Linux**: Run as non-root user with restricted permissions
- **macOS**: Use dedicated user account for launchd

## 🔄 Update and Maintenance Procedures

### Automated Updates (Windows)
Use the provided PowerShell script:
```powershell
.\service-manager.ps1 -Action update
```

### Manual Updates (Cross-Platform)
```bash
# 1. Stop the service
sudo systemctl stop messagebroadcast  # Linux
# or
sudo launchctl stop com.messagebroadcast.server  # macOS

# 2. Backup current version
sudo cp -r /opt/messagebroadcast /opt/messagebroadcast.backup

# 3. Deploy new version
sudo cp -r ./publish/linux-x64/* /opt/messagebroadcast/
sudo chown -R messagebroadcast:messagebroadcast /opt/messagebroadcast

# 4. Start the service
sudo systemctl start messagebroadcast  # Linux
# or
sudo launchctl start com.messagebroadcast.server  # macOS

# 5. Verify deployment
curl http://localhost:5001/health
```

### Rollback Procedures
```bash
# If update fails, rollback to previous version
sudo systemctl stop messagebroadcast
sudo rm -rf /opt/messagebroadcast
sudo mv /opt/messagebroadcast.backup /opt/messagebroadcast
sudo systemctl start messagebroadcast
```

---

This deployment guide provides comprehensive coverage for production deployment across all major platforms. For development setup and API usage, refer to the main [README](../README.md) and [API Reference](api-reference.md).