using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MessageBroadcast.Server.Services;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, DateTime> _connections = new();

    public void AddConnection(string connectionId)
    {
        _connections[connectionId] = DateTime.UtcNow;
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public int GetConnectionCount()
    {
        return _connections.Count;
    }

    public IEnumerable<string> GetAllConnectionIds()
    {
        return _connections.Keys;
    }

    public bool IsConnected(string connectionId)
    {
        return _connections.ContainsKey(connectionId);
    }

    // Get connection statistics for monitoring
    public Dictionary<string, object> GetConnectionStats()
    {
        return new Dictionary<string, object>
        {
            { "TotalConnections", _connections.Count },
            { "Timestamp", DateTime.UtcNow }
        };
    }
}