using System;
using System.Threading.Tasks;
using Xunit;
using MessageBroadcast.Client;
using MessageBroadcast.Client.Interfaces;
using MessageBroadcast.Client.Models;

namespace MessageBroadcast.Client.Tests
{
    public class ClientTests
    {
        private readonly IMessagePublisher _publisher;
        private readonly IMessageSubscriber _subscriber;

        public ClientTests()
        {
            _publisher = new MessageBroadcastClient(); // Assuming MessageBroadcastClient implements IMessagePublisher
            _subscriber = new MessageBroadcastClient(); // Assuming MessageBroadcastClient implements IMessageSubscriber
        }

        [Fact]
        public async Task PublishMessage_ShouldSendMessageToSubscribers()
        {
            // Arrange
            var message = new BroadcastMessage
            {
                Content = "Test Message",
                SenderId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Act
            await _publisher.PublishMessage(message);

            // Assert
            // Here you would typically verify that the message was received by subscribers
            // This might involve mocking the subscriber's behavior or checking a shared state
        }

        [Fact]
        public async Task SubscribeToMessages_ShouldReceiveMessages()
        {
            // Arrange
            var receivedMessage = default(BroadcastMessage);
            _subscriber.MessageReceived += (sender, message) => receivedMessage = message;

            // Act
            await _subscriber.SubscribeToMessages();

            // Simulate publishing a message
            var message = new BroadcastMessage
            {
                Content = "Test Message",
                SenderId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow
            };
            await _publisher.PublishMessage(message);

            // Wait for a moment to ensure the message is received
            await Task.Delay(100);

            // Assert
            Assert.NotNull(receivedMessage);
            Assert.Equal(message.Content, receivedMessage.Content);
        }
    }
}