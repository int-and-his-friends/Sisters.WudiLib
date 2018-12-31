using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sisters.WudiLib.WebSocket.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var cqWebSocketEvent = new CqHttpWebSocketEvent("");
            var httpApiClient = new HttpApiClient();
            cqWebSocketEvent.ApiClient = httpApiClient;
            cqWebSocketEvent.MessageEvent += (api, e) =>
            {
                Console.WriteLine(e.Content.Text);
                Console.WriteLine(api is null);
            };
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Available: {0}, Listening {1}", cqWebSocketEvent.IsAvailable, cqWebSocketEvent.IsListening);
                }
            });
            Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            var cancellationTokenSource = new CancellationTokenSource();
            cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
            Console.ReadLine();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
            Task.Delay(-1).Wait();
        }
    }
}
