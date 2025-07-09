using System;
using System.Threading;
using System.Threading.Tasks;
using MessageBroadcast.Client.Models;

namespace MessageBroadcast.Client.Interfaces;

public interface IMessageSubscriber
{
    Task SubscribeToMessagesAsync(Action<BroadcastMessage> onMessageReceived);
    Task SubscribeToMessagesAsync(Func<BroadcastMessage, Task> onMessageReceived, CancellationToken cancellationToken = default);
}