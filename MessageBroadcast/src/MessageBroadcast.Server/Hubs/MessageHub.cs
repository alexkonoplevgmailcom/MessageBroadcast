using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MessageBroadcast.Server.Services;
using MessageBroadcast.Server.Models;

namespace MessageBroadcast.Server.Hubs;

public class MessageHub : Hub
{
    private readonly BroadcastService _broadcastService;
    private readonly ConnectionManager _connectionManager;
    private readonly MessageProcessor _messageProcessor;

    public MessageHub(BroadcastService broadcastService, ConnectionManager connectionManager, MessageProcessor messageProcessor)
    {
        _broadcastService = broadcastService;
        _connectionManager = connectionManager;
        _messageProcessor = messageProcessor;
    }

    // Publisher sends messages via this method
    public async Task PublishMessage(string content, string senderId)
    {
        // Minimal processing - just validate and broadcast immediately
        if (string.IsNullOrEmpty(content))
            return;

        var message = new Message
        {
            Content = content,
            SenderId = senderId ?? Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        };

        // Fire-and-forget broadcasting - no transformation
        await _broadcastService.BroadcastMessageAsync(message);
    }

    // Subscribers join the broadcast group
    public async Task JoinBroadcastGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Subscribers");
        _connectionManager.AddConnection(Context.ConnectionId);
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _connectionManager.RemoveConnection(Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Subscribers");
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}