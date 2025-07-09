using MessageBroadcast.Client;
using MessageBroadcast.Client.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace MessageBroadcast.LoadTest
{
    public class LoadTestRunner
    {
        private readonly string _serverUrl;
        private readonly ConcurrentQueue<string> _receivedMessages = new();
        private readonly ConcurrentDictionary<string, DateTime> _sentMessages = new();
        private volatile int _totalMessagesSent = 0;
        private volatile int _totalMessagesReceived = 0;
        private readonly object _statsLock = new object();

        public LoadTestRunner(string serverUrl = "http://localhost:5001")
        {
            _serverUrl = serverUrl;
        }

        public async Task RunLoadTestAsync(int numPublishers, int numSubscribers, int messagesPerPublisher, int durationSeconds)
        {
            Console.WriteLine("=== MessageBroadcast Load Test ===");
            Console.WriteLine($"Server URL: {_serverUrl}");
            Console.WriteLine($"Publishers: {numPublishers}");
            Console.WriteLine($"Subscribers: {numSubscribers}");
            Console.WriteLine($"Messages per Publisher: {messagesPerPublisher}");
            Console.WriteLine($"Test Duration: {durationSeconds} seconds");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));

            // Start subscribers first
            var subscriberTasks = new Task[numSubscribers];
            for (int i = 0; i < numSubscribers; i++)
            {
                int subscriberId = i;
                subscriberTasks[i] = RunSubscriberAsync(subscriberId, cts.Token);
            }

            // Wait a moment for subscribers to connect
            await Task.Delay(2000);

            // Start publishers
            var publisherTasks = new Task[numPublishers];
            for (int i = 0; i < numPublishers; i++)
            {
                int publisherId = i;
                publisherTasks[i] = RunPublisherAsync(publisherId, messagesPerPublisher, cts.Token);
            }

            // Start stats monitoring
            var statsTask = MonitorStatsAsync(cts.Token);

            try
            {
                // Wait for all publishers to complete or timeout
                await Task.WhenAll(publisherTasks);
                
                // Give subscribers time to receive remaining messages
                await Task.Delay(2000);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Test duration reached, stopping...");
            }

            cts.Cancel();

            try
            {
                await Task.WhenAll(subscriberTasks);
                await statsTask;
            }
            catch (OperationCanceledException) { }

            stopwatch.Stop();

            // Final statistics
            PrintFinalStats(stopwatch.Elapsed, numPublishers * messagesPerPublisher);
        }

        private async Task RunPublisherAsync(int publisherId, int messageCount, CancellationToken cancellationToken)
        {
            var senderId = $"LoadTest-Publisher-{publisherId}";
            
            try
            {
                await using var client = new MessageBroadcastClient(_serverUrl);
                await client.ConnectAsync();
                
                Console.WriteLine($"Publisher {publisherId} connected");

                for (int i = 0; i < messageCount && !cancellationToken.IsCancellationRequested; i++)
                {
                    var messageId = $"{senderId}-{i}";
                    var content = $"Load test message {i} from {senderId}";
                    var message = new BroadcastMessage(content, senderId);

                    var sendTime = DateTime.UtcNow;
                    _sentMessages[messageId] = sendTime;

                    await client.PublishMessageAsync(message);
                    Interlocked.Increment(ref _totalMessagesSent);

                    // Small delay to control message rate
                    await Task.Delay(10, cancellationToken);
                }

                Console.WriteLine($"Publisher {publisherId} completed sending {messageCount} messages");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Publisher {publisherId} error: {ex.Message}");
            }
        }

        private async Task RunSubscriberAsync(int subscriberId, CancellationToken cancellationToken)
        {
            try
            {
                await using var client = new MessageBroadcastClient(_serverUrl);
                await client.ConnectAsync();

                await client.SubscribeToMessagesAsync(message =>
                {
                    var receivedTime = DateTime.UtcNow;
                    var messageInfo = $"{receivedTime:HH:mm:ss.fff} - {message.SenderId}: {message.Content}";
                    _receivedMessages.Enqueue(messageInfo);
                    Interlocked.Increment(ref _totalMessagesReceived);
                });

                Console.WriteLine($"Subscriber {subscriberId} connected and listening");

                // Keep subscriber alive until cancellation
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Subscriber {subscriberId} stopping");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Subscriber {subscriberId} error: {ex.Message}");
            }
        }

        private async Task MonitorStatsAsync(CancellationToken cancellationToken)
        {
            var lastSent = 0;
            var lastReceived = 0;
            var lastTime = DateTime.UtcNow;

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(5000, cancellationToken);

                var currentTime = DateTime.UtcNow;
                var currentSent = _totalMessagesSent;
                var currentReceived = _totalMessagesReceived;

                var elapsed = (currentTime - lastTime).TotalSeconds;
                var sentRate = (currentSent - lastSent) / elapsed;
                var receivedRate = (currentReceived - lastReceived) / elapsed;

                lock (_statsLock)
                {
                    Console.WriteLine($"[{currentTime:HH:mm:ss}] Sent: {currentSent} ({sentRate:F1}/s) | Received: {currentReceived} ({receivedRate:F1}/s)");
                }

                lastSent = currentSent;
                lastReceived = currentReceived;
                lastTime = currentTime;
            }
        }

        private void PrintFinalStats(TimeSpan elapsed, int expectedMessages)
        {
            Console.WriteLine();
            Console.WriteLine("=== LOAD TEST RESULTS ===");
            Console.WriteLine($"Test Duration: {elapsed.TotalSeconds:F2} seconds");
            Console.WriteLine($"Expected Messages: {expectedMessages:N0}");
            Console.WriteLine($"Messages Sent: {_totalMessagesSent:N0}");
            Console.WriteLine($"Messages Received: {_totalMessagesReceived:N0}");
            Console.WriteLine($"Send Rate: {_totalMessagesSent / elapsed.TotalSeconds:F2} messages/second");
            Console.WriteLine($"Receive Rate: {_totalMessagesReceived / elapsed.TotalSeconds:F2} messages/second");
            Console.WriteLine($"Delivery Rate: {(double)_totalMessagesReceived / _totalMessagesSent * 100:F1}%");
            Console.WriteLine($"Average Latency: Measured in real-time subscriber logs");
            
            if (_totalMessagesReceived >= _totalMessagesSent * 0.95)
            {
                Console.WriteLine("✅ SUCCESS: High delivery rate achieved!");
            }
            else
            {
                Console.WriteLine("⚠️  WARNING: Some message loss detected");
            }

            if (_totalMessagesSent / elapsed.TotalSeconds >= 100)
            {
                Console.WriteLine("✅ SUCCESS: High throughput achieved (>100 msg/s)!");
            }
            else
            {
                Console.WriteLine("ℹ️  INFO: Throughput is moderate");
            }
        }
    }
}
