using MessageBroadcast.Client;
using MessageBroadcast.Client.Models;
using System;

#nullable enable

Console.WriteLine("MessageBroadcast Subscriber Sample");
Console.WriteLine("==================================");

var serverUrl = "http://localhost:5001";
var subscriberId = Environment.MachineName + "-Subscriber";

await using var client = new MessageBroadcastClient(serverUrl);

try
{
    Console.WriteLine($"Connecting to server at {serverUrl}...");
    await client.ConnectAsync();
    Console.WriteLine("Connected successfully!");
    
    // Subscribe to messages
    await client.SubscribeToMessagesAsync(message =>
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] From {message.SenderId}: {message.Content}");
    });
    
    Console.WriteLine($"Subscriber '{subscriberId}' is listening for messages...");
    Console.WriteLine("Press any key to stop...");
    
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("Subscriber stopped.");