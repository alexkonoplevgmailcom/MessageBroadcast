namespace MessageBroadcast.Server.Configuration;

public class ServerConfig
{
    public int Port { get; set; } = 5000;
    public bool EnableDetailedErrors { get; set; } = false;
    public int KeepAliveIntervalSeconds { get; set; } = 15;
    public int ClientTimeoutIntervalSeconds { get; set; } = 30;
    public int MaxConcurrentConnections { get; set; } = 1000;
    public bool LogConnections { get; set; } = true;
}