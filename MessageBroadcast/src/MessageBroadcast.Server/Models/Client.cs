public class Client
{
    public string ClientId { get; set; }
    public string ConnectionId { get; set; }

    public Client(string clientId, string connectionId)
    {
        ClientId = clientId;
        ConnectionId = connectionId;
    }
}