using System;
using System.Threading.Tasks;
using MessageBroadcast.Server.Models;
using MessageBroadcast.Shared.Protocols;

namespace MessageBroadcast.Server.Services;

public class MessageProcessor
{
    private readonly BroadcastService _broadcastService;

    public MessageProcessor(BroadcastService broadcastService)
    {
        _broadcastService = broadcastService;
    }

    // Minimal processing - validate format and immediately queue for broadcasting
    public async Task ProcessMessageAsync(string rawMessage, string senderId)
    {
        try
        {
            // Basic validation only - no transformation
            if (string.IsNullOrWhiteSpace(rawMessage))
                return;

            var message = new Message
            {
                Content = rawMessage,
                SenderId = senderId,
                Timestamp = DateTime.UtcNow
            };

            // Immediate broadcasting without buffering
            await _broadcastService.BroadcastMessageAsync(message);
        }
        catch (Exception ex)
        {
            // Log error but don't block the pipeline
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }

    // Alternative method for processing with protocol formatting
    public async Task ProcessFormattedMessageAsync(string formattedMessage)
    {
        try
        {
            var (senderId, content, timestamp) = MessageProtocol.ParseMessage(formattedMessage);
            
            var message = new Message
            {
                Content = content,
                SenderId = senderId,
                Timestamp = timestamp
            };

            await _broadcastService.BroadcastMessageAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing formatted message: {ex.Message}");
        }
    }

    // Process message object directly
    public async Task ProcessMessageAsync(Message message)
    {
        if (ValidateMessage(message))
        {
            await _broadcastService.BroadcastMessageAsync(message);
        }
        else
        {
            throw new ArgumentException("Invalid message format.");
        }
    }

    private bool ValidateMessage(Message message)
    {
        return !string.IsNullOrEmpty(message.Content) && !string.IsNullOrEmpty(message.SenderId);
    }
}