using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MessageBroadcast.Server.Hubs;
using MessageBroadcast.Server.Models;

namespace MessageBroadcast.Server.Services;

public class BroadcastService
{
    private readonly IHubContext<MessageHub> _hubContext;
    private readonly ConnectionManager _connectionManager;

    public BroadcastService(IHubContext<MessageHub> hubContext, ConnectionManager connectionManager)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
    }

    // Fire-and-forget broadcasting - optimized for speed
    public async Task BroadcastMessageAsync(Message message)
    {
        // Zero-copy broadcasting using SignalR groups
        // No transformation, no buffering, immediate broadcast
        await _hubContext.Clients.Group("Subscribers").SendAsync("ReceiveMessage", new
        {
            content = message.Content,
            senderId = message.SenderId,
            timestamp = message.Timestamp
        });
    }

    // Alternative method for broadcasting to all connected clients
    public async Task BroadcastToAllAsync(Message message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", new
        {
            content = message.Content,
            senderId = message.SenderId,
            timestamp = message.Timestamp
        });
    }
}