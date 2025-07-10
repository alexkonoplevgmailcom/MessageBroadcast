using MessageBroadcast.Client;
using MessageBroadcast.Client.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Linq;

#nullable enable

namespace MessageBroadcast.LoadTest
{
    public class LoadTestRunner
    {
        private readonly string _serverUrl;
        private readonly ConcurrentQueue<string> _receivedMessages = new();
        private readonly ConcurrentQueue<TestMetric> _metrics = new();
        private readonly ConcurrentDictionary<string, DateTime> _sentMessages = new();
        private volatile int _totalMessagesSent = 0;
        private volatile int _totalMessagesReceived = 0;
        private readonly object _statsLock = new object();
        private DateTime _testStartTime;
        private TimeSpan _testDuration;
        private int _numPublishers;
        private int _numSubscribers;
        private int _messagesPerPublisher;

        public LoadTestRunner(string serverUrl = "http://localhost:5001")
        {
            _serverUrl = serverUrl;
        }

        public async Task RunLoadTestAsync(int numPublishers, int numSubscribers, int messagesPerPublisher, int durationSeconds)
        {
            _testStartTime = DateTime.UtcNow;
            _numPublishers = numPublishers;
            _numSubscribers = numSubscribers;
            _messagesPerPublisher = messagesPerPublisher;

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
            _testDuration = stopwatch.Elapsed;

            // Final statistics
            PrintFinalStats(stopwatch.Elapsed, numPublishers * messagesPerPublisher);
            
            // Generate HTML report
            await GenerateHtmlReportAsync();
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

                // Store metrics for HTML report
                _metrics.Enqueue(new TestMetric
                {
                    Timestamp = currentTime,
                    MessagesSent = currentSent,
                    MessagesReceived = currentReceived,
                    SendRate = sentRate,
                    ReceiveRate = receivedRate
                });

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
                Console.WriteLine("[SUCCESS] High delivery rate achieved!");
            }
            else
            {
                Console.WriteLine("[WARNING] Some message loss detected");
            }

            if (_totalMessagesSent / elapsed.TotalSeconds >= 100)
            {
                Console.WriteLine("[SUCCESS] High throughput achieved (>100 msg/s)!");
            }
            else
            {
                Console.WriteLine("[INFO] Throughput is moderate");
            }
        }

        private async Task GenerateHtmlReportAsync()
        {
            var reportPath = Path.Combine(Directory.GetCurrentDirectory(), $"LoadTestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            
            var deliveryRate = _totalMessagesSent > 0 ? (double)_totalMessagesReceived / _totalMessagesSent * 100 : 0;
            var avgThroughput = _testDuration.TotalSeconds > 0 ? _totalMessagesSent / _testDuration.TotalSeconds : 0;
            
            var html = GenerateHtmlContent(deliveryRate, avgThroughput);
            
            await File.WriteAllTextAsync(reportPath, html);
            
            Console.WriteLine();
            Console.WriteLine($"HTML Report generated: {reportPath}");
            
            // Open in default browser
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = reportPath,
                    UseShellExecute = true
                });
                Console.WriteLine("Opening report in default browser...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open browser automatically: {ex.Message}");
                Console.WriteLine($"Please open the report manually: {reportPath}");
            }
        }

        private string GenerateHtmlContent(double deliveryRate, double avgThroughput)
        {
            var metricsData = _metrics.ToArray();
            var chartData = string.Join(",", metricsData.Select(m => 
                $"{{timestamp: '{m.Timestamp:HH:mm:ss}', sent: {m.MessagesSent}, received: {m.MessagesReceived}, sendRate: {m.SendRate:F1}, receiveRate: {m.ReceiveRate:F1}}}"));

            var statusColor = deliveryRate >= 95 ? "#4CAF50" : (deliveryRate >= 90 ? "#FF9800" : "#F44336");
            var statusText = deliveryRate >= 95 ? "EXCELLENT" : (deliveryRate >= 90 ? "GOOD" : "NEEDS IMPROVEMENT");

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>MessageBroadcast Load Test Report</title>
    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .header h1 {{ color: #2196F3; margin-bottom: 10px; }}
        .status-badge {{ display: inline-block; padding: 8px 16px; border-radius: 20px; color: white; font-weight: bold; background-color: {statusColor}; }}
        .stats-grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin-bottom: 30px; }}
        .stat-card {{ background: #f8f9fa; padding: 20px; border-radius: 8px; border-left: 4px solid #2196F3; }}
        .stat-card h3 {{ margin: 0 0 10px 0; color: #333; font-size: 14px; text-transform: uppercase; }}
        .stat-card .value {{ font-size: 24px; font-weight: bold; color: #2196F3; }}
        .stat-card .unit {{ font-size: 14px; color: #666; }}
        .chart-container {{ margin: 30px 0; }}
        .chart-container canvas {{ max-height: 400px; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
        .test-params {{ background: #e3f2fd; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .test-params h3 {{ margin: 0 0 10px 0; color: #1976d2; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>MessageBroadcast Load Test Report</h1>
            <div class='status-badge'>{statusText}</div>
            <p>Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
        </div>

        <div class='test-params'>
            <h3>Test Configuration</h3>
            <p><strong>Server URL:</strong> {_serverUrl}</p>
            <p><strong>Publishers:</strong> {_numPublishers} | <strong>Subscribers:</strong> {_numSubscribers} | <strong>Messages per Publisher:</strong> {_messagesPerPublisher}</p>
            <p><strong>Test Duration:</strong> {_testDuration.TotalSeconds:F2} seconds</p>
        </div>

        <div class='stats-grid'>
            <div class='stat-card'>
                <h3>Messages Sent</h3>
                <div class='value'>{_totalMessagesSent:N0}</div>
                <div class='unit'>total messages</div>
            </div>
            <div class='stat-card'>
                <h3>Messages Received</h3>
                <div class='value'>{_totalMessagesReceived:N0}</div>
                <div class='unit'>total messages</div>
            </div>
            <div class='stat-card'>
                <h3>Delivery Rate</h3>
                <div class='value'>{deliveryRate:F1}%</div>
                <div class='unit'>success rate</div>
            </div>
            <div class='stat-card'>
                <h3>Average Throughput</h3>
                <div class='value'>{avgThroughput:F1}</div>
                <div class='unit'>messages/second</div>
            </div>
            <div class='stat-card'>
                <h3>Peak Send Rate</h3>
                <div class='value'>{(metricsData.Length > 0 ? metricsData.Max(m => m.SendRate) : 0):F1}</div>
                <div class='unit'>messages/second</div>
            </div>
            <div class='stat-card'>
                <h3>Peak Receive Rate</h3>
                <div class='value'>{(metricsData.Length > 0 ? metricsData.Max(m => m.ReceiveRate) : 0):F1}</div>
                <div class='unit'>messages/second</div>
            </div>
        </div>

        <div class='chart-container'>
            <h3>Message Throughput Over Time</h3>
            <canvas id='throughputChart'></canvas>
        </div>

        <div class='chart-container'>
            <h3>Cumulative Messages</h3>
            <canvas id='cumulativeChart'></canvas>
        </div>

        <div class='footer'>
            <p>MessageBroadcast Load Test Report - Generated by .NET 8 Load Testing Framework</p>
        </div>
    </div>

    <script>
        const data = [{chartData}];
        
        // Throughput Chart
        const throughputCtx = document.getElementById('throughputChart').getContext('2d');
        new Chart(throughputCtx, {{
            type: 'line',
            data: {{
                labels: data.map(d => d.timestamp),
                datasets: [{{
                    label: 'Send Rate (msg/s)',
                    data: data.map(d => d.sendRate),
                    borderColor: '#2196F3',
                    backgroundColor: 'rgba(33, 150, 243, 0.1)',
                    tension: 0.1
                }}, {{
                    label: 'Receive Rate (msg/s)',
                    data: data.map(d => d.receiveRate),
                    borderColor: '#4CAF50',
                    backgroundColor: 'rgba(76, 175, 80, 0.1)',
                    tension: 0.1
                }}]
            }},
            options: {{
                responsive: true,
                maintainAspectRatio: false,
                scales: {{
                    y: {{
                        beginAtZero: true,
                        title: {{ display: true, text: 'Messages per Second' }}
                    }}
                }}
            }}
        }});

        // Cumulative Chart
        const cumulativeCtx = document.getElementById('cumulativeChart').getContext('2d');
        new Chart(cumulativeCtx, {{
            type: 'line',
            data: {{
                labels: data.map(d => d.timestamp),
                datasets: [{{
                    label: 'Messages Sent',
                    data: data.map(d => d.sent),
                    borderColor: '#FF9800',
                    backgroundColor: 'rgba(255, 152, 0, 0.1)',
                    tension: 0.1
                }}, {{
                    label: 'Messages Received',
                    data: data.map(d => d.received),
                    borderColor: '#9C27B0',
                    backgroundColor: 'rgba(156, 39, 176, 0.1)',
                    tension: 0.1
                }}]
            }},
            options: {{
                responsive: true,
                maintainAspectRatio: false,
                scales: {{
                    y: {{
                        beginAtZero: true,
                        title: {{ display: true, text: 'Total Messages' }}
                    }}
                }}
            }}
        }});
    </script>
</body>
</html>";
        }
    }

    public class TestMetric
    {
        public DateTime Timestamp { get; set; }
        public int MessagesSent { get; set; }
        public int MessagesReceived { get; set; }
        public double SendRate { get; set; }
        public double ReceiveRate { get; set; }
    }
}
