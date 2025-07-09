# MessageBroadcast System

A high-performance, standalone message broadcasting system built with .NET 8 and SignalR. Designed to run on Windows and macOS without external network dependencies, supporting hundreds of messages per second in a fire-and-forget pattern.

## Key Features

- 🚀 **High Performance**: Handles hundreds of messages per second
- 🔥 **Fire-and-Forget**: No guaranteed delivery, optimized for speed
- 🖥️ **Standalone**: Runs without external network access
- 🔄 **Zero Transformation**: Messages broadcast immediately without processing
- 🌐 **Cross-Platform**: Windows and macOS support
- ⚡ **SignalR WebSockets**: Efficient real-time communication
- 📦 **Self-Contained**: Single executable deployment

## Architecture

### Components

- **MessageBroadcast.Server**: SignalR-based server for message broadcasting
- **MessageBroadcast.Client**: Client library for publishers and subscribers
- **MessageBroadcast.Shared**: Common contracts and protocols

### Protocol Choice: SignalR with WebSockets

SignalR was chosen as the optimal protocol because:
- ✅ Built-in connection management and reconnection
- ✅ Efficient one-to-many broadcasting
- ✅ Automatic WebSocket/Server-Sent Events fallback
- ✅ Minimal latency for fire-and-forget scenarios
- ✅ No external dependencies

## Quick Start

### 1. Build the Solution

```bash
cd MessageBroadcast

# Restore packages
dotnet restore src/MessageBroadcast.Shared
dotnet restore src/MessageBroadcast.Server  
dotnet restore src/MessageBroadcast.Client

# Build projects
dotnet build src/MessageBroadcast.Server
dotnet build src/MessageBroadcast.Client
```

### 2. Run the Server

```bash
cd src/MessageBroadcast.Server
dotnet run
```

The server will start on `http://localhost:5000` by default.

### 3. Run Sample Applications

**Publisher:**
```bash
cd samples/Publisher.Sample
dotnet run
```

**Subscriber:**
```bash  
cd samples/Subscriber.Sample
dotnet run
```

## Usage Examples

### Publisher
```csharp
using var client = new MessageBroadcastClient("http://localhost:5000");
await client.ConnectAsync();

// Fire-and-forget message publishing
await client.PublishMessageAsync("Hello World!", "Publisher1");
```

### Subscriber
```csharp
using var client = new MessageBroadcastClient("http://localhost:5000");
await client.ConnectAsync();

// Real-time message subscription
await client.SubscribeToMessagesAsync(message =>
{
    Console.WriteLine($"From {message.SenderId}: {message.Content}");
});
```

## Configuration

### Server Settings (`appsettings.json`)
```json
{
  "Server": {
    "Port": 5000,
    "MaxConcurrentConnections": 1000,
    "KeepAliveIntervalSeconds": 15,
    "ClientTimeoutIntervalSeconds": 30
  }
}
```

## Performance Optimizations

- **Asynchronous Processing**: All operations are async
- **Zero-Copy Broadcasting**: Direct message forwarding
- **SignalR Groups**: Efficient subscriber management  
- **No Buffering**: Immediate message broadcast
- **Minimal Serialization**: Lightweight JSON objects

## Deployment

### Self-Contained Deployment
```bash
# Windows
dotnet publish src/MessageBroadcast.Server -c Release -o ./deploy/windows --self-contained -r win-x64 --single-file

# macOS  
dotnet publish src/MessageBroadcast.Server -c Release -o ./deploy/macos --self-contained -r osx-x64 --single-file
```

### Framework-Dependent Deployment
```bash
dotnet publish src/MessageBroadcast.Server -c Release -o ./deploy/portable
```

## Documentation

- [Architecture Guide](docs/architecture.md) - Detailed system design
- [Deployment Guide](docs/deployment.md) - Installation and configuration
- [API Reference](docs/api-reference.md) - Client library documentation

## System Requirements

- **.NET 8 Runtime** (or SDK for building)
- **Memory**: 512MB minimum, 1GB recommended for high throughput
- **Network**: Local network access (no internet required)
- **OS**: Windows 10+ or macOS 10.15+

## Performance Characteristics

- **Throughput**: Hundreds of messages per second
- **Latency**: Sub-millisecond message broadcasting
- **Connections**: Up to 1000+ concurrent subscribers
- **Memory**: Minimal buffering, low memory footprint
- **CPU**: Low CPU usage due to zero-transformation design

## License

This project is licensed under the MIT License.