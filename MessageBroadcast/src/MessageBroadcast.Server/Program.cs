using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MessageBroadcast.Server.Configuration;
using MessageBroadcast.Server.Hubs;
using MessageBroadcast.Server.Services;

namespace MessageBroadcast.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure services
        builder.Services.Configure<ServerConfig>(builder.Configuration.GetSection("Server"));
        builder.Services.AddSingleton<ConnectionManager>();
        builder.Services.AddSingleton<MessageProcessor>();
        builder.Services.AddSingleton<BroadcastService>();

        // Add SignalR
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = false; // Optimize for performance
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

        // Configure CORS for standalone operation
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        app.UseCors();
        app.UseRouting();

        // Map SignalR hub
        app.MapHub<MessageHub>("/messagehub");

        // Health check endpoint
        app.MapGet("/health", () => "OK");

        // Start server
        var serverConfig = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServerConfig>>().Value;
        var port = serverConfig?.Port ?? 5000;

        app.Urls.Add($"http://localhost:{port}");
        app.Urls.Add($"http://0.0.0.0:{port}");

        Console.WriteLine($"MessageBroadcast Server starting on port {port}");
        Console.WriteLine("Press Ctrl+C to shutdown");

        app.Run();
    }
}