using MessageBroadcast.LoadTest;
using System;
using System.Threading.Tasks;

namespace MessageBroadcast.LoadTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("MessageBroadcast Load Testing Tool");
            Console.WriteLine("================================");
            Console.WriteLine();

            // Parse command line arguments or use defaults
            var serverUrl = args.Length > 0 ? args[0] : "http://localhost:5001";
            var numPublishers = args.Length > 1 ? int.Parse(args[1]) : 5;
            var numSubscribers = args.Length > 2 ? int.Parse(args[2]) : 10;
            var messagesPerPublisher = args.Length > 3 ? int.Parse(args[3]) : 100;
            var durationSeconds = args.Length > 4 ? int.Parse(args[4]) : 60;

            Console.WriteLine("Usage: LoadTest [serverUrl] [publishers] [subscribers] [messagesPerPublisher] [durationSeconds]");
            Console.WriteLine($"Using: {serverUrl} {numPublishers} {numSubscribers} {messagesPerPublisher} {durationSeconds}");
            Console.WriteLine();

            var loadTester = new LoadTestRunner(serverUrl);

            try
            {
                await loadTester.RunLoadTestAsync(numPublishers, numSubscribers, messagesPerPublisher, durationSeconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load test failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }

            Console.WriteLine();
            Console.WriteLine("Load test completed. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
