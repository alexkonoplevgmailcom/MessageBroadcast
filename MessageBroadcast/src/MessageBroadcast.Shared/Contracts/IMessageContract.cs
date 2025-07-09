using System;

namespace MessageBroadcast.Shared.Contracts;

public interface IMessageContract
{
    string Content { get; set; }
    string SenderId { get; set; }
    DateTime Timestamp { get; set; }
}