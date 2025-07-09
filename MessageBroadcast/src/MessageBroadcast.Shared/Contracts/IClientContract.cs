namespace MessageBroadcast.Shared.Contracts
{
    public interface IClientContract
    {
        void Connect(string clientId);
        void Disconnect(string clientId);
        void Subscribe(string topic);
        void Unsubscribe(string topic);
        void ReceiveMessage(string topic, string message);
    }
}