using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MessageBroadcast.Client.Interfaces;
using MessageBroadcast.Client.Models;
using MessageBroadcast.Shared.Protocols;

namespace MessageBroadcast.Client;

public class MessageBroadcastClient : IMessagePublisher, IMessageSubscriber, IAsyncDisposable
{
    private readonly string _serverUrl;
    private HubConnection _connection;
    private bool _isConnected;

    public MessageBroadcastClient(string serverUrl)
    {
        _serverUrl = serverUrl.TrimEnd('/');
    }

    // Connect to the SignalR hub
    public async Task ConnectAsync()
    {
        if (_connection != null && _isConnected)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl($"{_serverUrl}/messagehub")
            .WithAutomaticReconnect() // Auto-reconnect for resilience
            .Build();

        await _connection.StartAsync();
        _isConnected = true;
    }

    // Publisher interface implementation
    public async Task PublishMessageAsync(string content, string senderId = null)
    {
        if (!_isConnected || _connection == null)
            throw new InvalidOperationException("Client not connected. Call ConnectAsync() first.");

        // Fire-and-forget publishing
        await _connection.InvokeAsync("PublishMessage", content, senderId ?? Environment.MachineName);
    }

    public async Task PublishMessageAsync(BroadcastMessage message)
    {
        await PublishMessageAsync(message.Content, message.SenderId);
    }

    // Subscriber interface implementation
    public async Task SubscribeToMessagesAsync(Action<BroadcastMessage> onMessageReceived)
    {
        if (!_isConnected || _connection == null)
            throw new InvalidOperationException("Client not connected. Call ConnectAsync() first.");

        // Join the subscribers group
        await _connection.InvokeAsync("JoinBroadcastGroup");

        // Register the message handler
        _connection.On<object>("ReceiveMessage", (messageData) =>
        {
            try
            {
                // Handle dynamic object from SignalR
                var json = System.Text.Json.JsonSerializer.Serialize(messageData);
                var data = System.Text.Json.JsonSerializer.Deserialize<MessageData>(json);
                
                if (data != null)
                {
                    var broadcastMessage = new BroadcastMessage
                    {
                        Content = data.content ?? string.Empty,
                        SenderId = data.senderId ?? string.Empty,
                        Timestamp = data.timestamp
                    };
                    
                    onMessageReceived(broadcastMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing received message: {ex.Message}");
            }
        });
    }

    // Async subscriber with CancellationToken support
    public async Task SubscribeToMessagesAsync(Func<BroadcastMessage, Task> onMessageReceived, CancellationToken cancellationToken = default)
    {
        if (!_isConnected || _connection == null)
            throw new InvalidOperationException("Client not connected. Call ConnectAsync() first.");

        await _connection.InvokeAsync("JoinBroadcastGroup", cancellationToken);

        _connection.On<object>("ReceiveMessage", async (messageData) =>
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(messageData);
                var data = System.Text.Json.JsonSerializer.Deserialize<MessageData>(json);
                
                if (data != null)
                {
                    var broadcastMessage = new BroadcastMessage
                    {
                        Content = data.content ?? string.Empty,
                        SenderId = data.senderId ?? string.Empty,
                        Timestamp = data.timestamp
                    };
                    
                    await onMessageReceived(broadcastMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing received message: {ex.Message}");
            }
        });
    }

    public bool IsConnected => _isConnected && _connection?.State == HubConnectionState.Connected;

    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
            _isConnected = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    // Internal class for deserializing SignalR messages
    private class MessageData
    {
        public string content { get; set; }
        public string senderId { get; set; }
        public DateTime timestamp { get; set; }
    }
}