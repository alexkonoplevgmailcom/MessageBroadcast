using System;

namespace MessageBroadcast.Server.Models;

public class Message
{
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public Message()
    {
        Timestamp = DateTime.UtcNow;
    }

    public Message(string content, string senderId)
    {
        Content = content;
        SenderId = senderId;
        Timestamp = DateTime.UtcNow;
    }
}