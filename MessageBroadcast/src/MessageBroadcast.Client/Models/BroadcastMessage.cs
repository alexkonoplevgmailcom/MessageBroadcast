using System;

namespace MessageBroadcast.Client.Models;

public class BroadcastMessage
{
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public BroadcastMessage()
    {
        Timestamp = DateTime.UtcNow;
    }

    public BroadcastMessage(string content, string senderId)
    {
        Content = content;
        SenderId = senderId;
        Timestamp = DateTime.UtcNow;
    }
}