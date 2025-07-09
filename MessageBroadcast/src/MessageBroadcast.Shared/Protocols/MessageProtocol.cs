using System;

namespace MessageBroadcast.Shared.Protocols;

public static class MessageProtocol
{
    public const string MessageType = "Message";
    private const char Delimiter = '|';

    // Optimized for fire-and-forget - no acknowledgments
    public static string FormatMessage(string senderId, string content)
    {
        return $"{MessageType}{Delimiter}{senderId}{Delimiter}{content}{Delimiter}{DateTime.UtcNow:O}";
    }

    public static (string senderId, string content, DateTime timestamp) ParseMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            throw new FormatException("Message cannot be null or empty.");

        var parts = message.Split(Delimiter);
        if (parts.Length != 4 || parts[0] != MessageType)
        {
            throw new FormatException("Invalid message format.");
        }

        if (!DateTime.TryParse(parts[3], out var timestamp))
        {
            timestamp = DateTime.UtcNow;
        }

        return (parts[1], parts[2], timestamp);
    }

    // Simple validation for high-throughput scenarios
    public static bool IsValidMessage(string message)
    {
        try
        {
            if (string.IsNullOrEmpty(message)) return false;
            var parts = message.Split(Delimiter);
            return parts.Length == 4 && parts[0] == MessageType;
        }
        catch
        {
            return false;
        }
    }

    // Create a lightweight message object for JSON serialization
    public static object CreateMessageObject(string senderId, string content)
    {
        return new
        {
            senderId,
            content,
            timestamp = DateTime.UtcNow
        };
    }
}