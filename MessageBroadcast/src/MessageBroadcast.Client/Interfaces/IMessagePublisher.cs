using System.Threading.Tasks;
using MessageBroadcast.Client.Models;

namespace MessageBroadcast.Client.Interfaces;

public interface IMessagePublisher
{
    Task PublishMessageAsync(string content, string senderId = null);
    Task PublishMessageAsync(BroadcastMessage message);
}