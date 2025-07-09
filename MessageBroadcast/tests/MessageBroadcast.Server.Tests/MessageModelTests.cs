using Xunit;
using MessageBroadcast.Server.Models;
using System;

namespace MessageBroadcast.Server.Tests
{
    public class MessageModelTests
    {
        [Fact]
        public void Message_ShouldHaveRequiredProperties()
        {
            // Arrange & Act
            var message = new Message
            {
                Content = "Test Content",
                SenderId = "TestSender",
                Timestamp = DateTime.UtcNow
            };

            // Assert
            Assert.NotNull(message.Content);
            Assert.NotNull(message.SenderId);
            Assert.True(message.Timestamp <= DateTime.UtcNow);
        }

        [Fact]
        public void Message_ShouldValidateNonEmptyContent()
        {
            // Arrange & Act
            var message = new Message
            {
                Content = "Valid Content",
                SenderId = "ValidSender", 
                Timestamp = DateTime.UtcNow
            };

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(message.Content));
            Assert.False(string.IsNullOrWhiteSpace(message.SenderId));
        }

        [Fact]
        public void Message_ShouldHaveValidTimestamp()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;
            
            // Act
            var message = new Message
            {
                Content = "Test",
                SenderId = "Sender",
                Timestamp = DateTime.UtcNow
            };
            
            var afterCreation = DateTime.UtcNow;

            // Assert
            Assert.True(message.Timestamp >= beforeCreation);
            Assert.True(message.Timestamp <= afterCreation);
        }
    }
}
