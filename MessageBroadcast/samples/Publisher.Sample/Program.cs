using MessageBroadcast.Client;
using MessageBroadcast.Client.Models;
using System;

#nullable enable

Console.WriteLine("MessageBroadcast Publisher Sample");
Console.WriteLine("================================");

var serverUrl = "http://localhost:5001";
var senderId = Environment.MachineName + "-Publisher";

await using var client = new MessageBroadcastClient(serverUrl);

try
{
    Console.WriteLine($"Connecting to server at {serverUrl}...");
    await client.ConnectAsync();
    Console.WriteLine("Connected successfully!");
    
    Console.WriteLine("Enter messages to broadcast (type 'quit' to exit):");
    
    string? input;
    int messageCount = 0;
    
    while ((input = Console.ReadLine()) != "quit" && input != null)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            messageCount++;
            var message = new BroadcastMessage(input, senderId);
            
            await client.PublishMessageAsync(message);
            Console.WriteLine($"[{messageCount}] Message sent: {input}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("Publisher stopped.");