# MessageBroadcast API Reference

## Overview

The MessageBroadcast system provides a high-performance, real-time message broadcasting solution using SignalR and .NET 8. This document details the complete API surface for both server and client components.

## 🏗️ System Architecture

The system consists of three main components:
- **Server**: SignalR hub for message broadcasting
- **Client Library**: Publisher and subscriber client implementations  
- **Shared Contracts**: Common interfaces and message protocols

## 🖥️ Server API

### MessageHub (SignalR Hub)

The core SignalR hub that handles real-time communication between publishers and subscribers.

**Hub Endpoint**: `/messagehub`

#### Hub Methods

##### `JoinGroup(string groupName)`
- **Description**: Joins a client to a specific message group
- **Parameters**: 
  - `groupName` (string): Name of the group to join
- **Returns**: `Task`
- **Usage**: Automatically called by client library

##### `LeaveGroup(string groupName)`
- **Description**: Removes a client from a message group
- **Parameters**:
  - `groupName` (string): Name of the group to leave  
- **Returns**: `Task`
- **Usage**: Automatically called by client library

#### Hub Events (Server → Client)

##### `ReceiveMessage(Message message)`
- **Description**: Broadcasts a message to all subscribers
- **Parameters**:
  - `message` (Message): The message object containing content and metadata
- **Triggered**: When a publisher sends a message

##### `NotifyConnectionUpdate(string connectionId, bool connected)`
- **Description**: Notifies about connection status changes
- **Parameters**:
  - `connectionId` (string): The SignalR connection identifier
  - `connected` (bool): Connection status (true = connected, false = disconnected)

### BroadcastService

Core service responsible for message processing and distribution.

#### Methods

##### `BroadcastMessageAsync(Message message)`
- **Description**: Processes and broadcasts a message to all connected subscribers
- **Parameters**:
  - `message` (Message): Message to broadcast
- **Returns**: `Task`
- **Features**: Zero-copy broadcasting for optimal performance

##### `GetConnectionCountAsync()`
- **Description**: Returns the current number of active connections
- **Returns**: `Task<int>`

### ConnectionManager

Manages SignalR connections and client tracking.

#### Methods

##### `AddConnectionAsync(string connectionId, Client client)`
- **Description**: Registers a new client connection
- **Parameters**:
  - `connectionId` (string): SignalR connection identifier
  - `client` (Client): Client information object
- **Returns**: `Task`

##### `RemoveConnectionAsync(string connectionId)`
- **Description**: Unregisters a client connection
- **Parameters**:
  - `connectionId` (string): Connection identifier to remove
- **Returns**: `Task`

##### `GetActiveConnectionsAsync()`
- **Description**: Retrieves all active client connections
- **Returns**: `Task<IEnumerable<Client>>`

## 📱 Client API

### MessageBroadcastClient

Main client class implementing both publisher and subscriber functionality.

#### Constructor

##### `MessageBroadcastClient(string serverUrl)`
- **Parameters**:
  - `serverUrl` (string): Base URL of the MessageBroadcast server
- **Example**: `new MessageBroadcastClient("http://localhost:5001")`

#### Connection Management

##### `ConnectAsync(CancellationToken cancellationToken = default)`
- **Description**: Establishes connection to the SignalR hub
- **Returns**: `Task`
- **Features**: 
  - Automatic reconnection on connection loss
  - Configurable retry policies
  - Connection state management

##### `DisconnectAsync()`
- **Description**: Gracefully disconnects from the server
- **Returns**: `Task`

##### `IsConnected`
- **Description**: Gets the current connection status
- **Returns**: `bool`

#### Publisher Interface (IMessagePublisher)

##### `PublishMessageAsync(string content, string senderId, CancellationToken cancellationToken = default)`
- **Description**: Publishes a message for broadcasting
- **Parameters**:
  - `content` (string): Message content
  - `senderId` (string): Identifier of the message sender
  - `cancellationToken` (CancellationToken): Optional cancellation token
- **Returns**: `Task`
- **Performance**: Fire-and-forget with minimal latency

##### `PublishMessageAsync(BroadcastMessage message, CancellationToken cancellationToken = default)`
- **Description**: Publishes a pre-constructed message object
- **Parameters**:
  - `message` (BroadcastMessage): Complete message object
  - `cancellationToken` (CancellationToken): Optional cancellation token
- **Returns**: `Task`

#### Subscriber Interface (IMessageSubscriber)

##### `SubscribeToMessagesAsync(Action<BroadcastMessage> onMessageReceived, CancellationToken cancellationToken = default)`
- **Description**: Subscribes to receive real-time messages
- **Parameters**:
  - `onMessageReceived` (Action<BroadcastMessage>): Callback for processing received messages
  - `cancellationToken` (CancellationToken): Optional cancellation token
- **Returns**: `Task`
- **Features**: Real-time message delivery with automatic reconnection

##### `UnsubscribeAsync()`
- **Description**: Stops receiving messages and leaves subscriber group
- **Returns**: `Task`

## 📋 Data Models

### Message (Server Model)

Core server-side message representation.

```csharp
public class Message
{
    public string Content { get; set; }      // Message content
    public string SenderId { get; set; }     // Publisher identifier  
    public DateTime Timestamp { get; set; }  // Server timestamp
    public string MessageId { get; set; }    // Unique message identifier
}
```

### BroadcastMessage (Client Model)

Client-side message representation mirroring the server model.

```csharp
public class BroadcastMessage
{
    public string Content { get; set; }      // Message content
    public string SenderId { get; set; }     // Publisher identifier
    public DateTime Timestamp { get; set; }  // Message timestamp
    public string MessageId { get; set; }    // Unique message identifier
}
```

### Client (Connection Model)

Represents a connected client for tracking purposes.

```csharp
public class Client
{
    public string ConnectionId { get; set; }   // SignalR connection ID
    public string ClientId { get; set; }       // Client identifier
    public DateTime ConnectedAt { get; set; }  // Connection timestamp
    public ClientType Type { get; set; }       // Publisher or Subscriber
}
```

## 🔗 Interfaces

### IMessagePublisher

```csharp
public interface IMessagePublisher
{
    Task PublishMessageAsync(string content, string senderId, CancellationToken cancellationToken = default);
    Task PublishMessageAsync(BroadcastMessage message, CancellationToken cancellationToken = default);
}
```

### IMessageSubscriber

```csharp
public interface IMessageSubscriber
{
    Task SubscribeToMessagesAsync(Action<BroadcastMessage> onMessageReceived, CancellationToken cancellationToken = default);
    Task UnsubscribeAsync();
}
```

## 🔧 Configuration

### Server Configuration (appsettings.json)

```json
{
  "Server": {
    "Port": 5001,
    "HttpsPort": 5002,
    "MaxConcurrentConnections": 1000,
    "KeepAliveIntervalSeconds": 15,
    "ClientTimeoutIntervalSeconds": 30,
    "EnableDetailedErrors": false,
    "LogConnections": true
  }
}
```

### Client Configuration

```csharp
var client = new MessageBroadcastClient("http://localhost:5001");

// Configure connection options
client.Connection.HandshakeTimeout = TimeSpan.FromSeconds(30);
client.Connection.KeepAliveInterval = TimeSpan.FromSeconds(15);
client.Connection.ServerTimeout = TimeSpan.FromSeconds(30);
```

## 🚨 Error Handling

### Common Exceptions

- **`HubException`**: SignalR hub-specific errors
- **`TimeoutException`**: Connection or operation timeout
- **`InvalidOperationException`**: Invalid client state operations
- **`ArgumentException`**: Invalid parameters

### Best Practices

```csharp
try
{
    await client.ConnectAsync();
    await client.PublishMessageAsync("Hello World", "Publisher1");
}
catch (HubException ex)
{
    // Handle SignalR-specific errors
    Console.WriteLine($"Hub error: {ex.Message}");
}
catch (TimeoutException ex)
{
    // Handle timeout scenarios
    Console.WriteLine($"Operation timed out: {ex.Message}");
}
```

## 📊 Performance Considerations

### Throughput Optimization
- Use `PublishMessageAsync` with pre-constructed `BroadcastMessage` objects
- Implement connection pooling for multiple publishers
- Configure appropriate buffer sizes for high-volume scenarios

### Memory Management
- Dispose of `MessageBroadcastClient` instances properly
- Use `CancellationToken` for long-running operations
- Monitor connection counts to prevent resource leaks

## 🔍 Health Monitoring

### Server Health Endpoint

```http
GET /health
Response: "OK" (200 OK)
```

### Connection Monitoring

```csharp
// Check connection status
if (client.IsConnected)
{
    // Perform operations
}

// Monitor connection events
client.Closed += async (error) =>
{
    Console.WriteLine("Connection lost, attempting to reconnect...");
    await client.ConnectAsync();
};
```

---

This API reference provides comprehensive coverage of the MessageBroadcast system's functionality. For implementation examples and deployment guidance, refer to the main [README](../README.md) and [Deployment Guide](deployment.md).