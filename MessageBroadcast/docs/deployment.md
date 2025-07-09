# MessageBroadcast Deployment Guide

## Overview
This guide covers deploying the MessageBroadcast server as a standalone application on Windows and macOS machines without external network dependencies.

## Prerequisites

### System Requirements
- **.NET 8 Runtime** (for running pre-built binaries)
- **Memory**: Minimum 512MB RAM (recommended 1GB for high throughput)
- **Disk Space**: 100MB for application files
- **Network**: Local network access (no internet required)

### Development Requirements (for building from source)
- **.NET 8 SDK**
- **Visual Studio 2022**, **VS Code**, or **JetBrains Rider**

## Building the Application

### From Source
```bash
# Clone or extract the source code
cd MessageBroadcast

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Build self-contained executables
dotnet publish src/MessageBroadcast.Server/MessageBroadcast.Server.csproj -c Release -o ./publish/server --self-contained true -r win-x64
dotnet publish src/MessageBroadcast.Server/MessageBroadcast.Server.csproj -c Release -o ./publish/server-mac --self-contained true -r osx-x64
```

## Deployment Options

### Option 1: Self-Contained Deployment (Recommended)
Self-contained deployments include the .NET runtime and can run without .NET installed on the target machine.

**Windows:**
```bash
dotnet publish src/MessageBroadcast.Server/MessageBroadcast.Server.csproj -c Release -o ./deploy/windows --self-contained true -r win-x64 --single-file
```

**macOS:**
```bash
dotnet publish src/MessageBroadcast.Server/MessageBroadcast.Server.csproj -c Release -o ./deploy/macos --self-contained true -r osx-x64 --single-file
```

### Option 2: Framework-Dependent Deployment
Requires .NET 8 runtime to be installed on the target machine.

```bash
dotnet publish src/MessageBroadcast.Server/MessageBroadcast.Server.csproj -c Release -o ./deploy/portable --no-self-contained
```

## Configuration

### Server Configuration
Edit `appsettings.json` in the deployment folder:

```json
{
  "Server": {
    "Port": 5000,
    "EnableDetailedErrors": false,
    "KeepAliveIntervalSeconds": 15,
    "ClientTimeoutIntervalSeconds": 30,
    "MaxConcurrentConnections": 1000,
    "LogConnections": true
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