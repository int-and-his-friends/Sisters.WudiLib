using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sisters.WudiLib.WebSocket.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var cqWebSocketEvent = new CqWebSocketEvent("");
            var httpApiClient = new HttpApiClient();
            cqWebSocketEvent.ApiClient = httpApiClient;
            cqWebSocketEvent.MessageEvent += (api, e) =>
            {
                Console.WriteLine(e.Content.Text);
                Console.WriteLine(api is null);
            };
            var cancellationTokenSource = new CancellationTokenSource();
            cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
            cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(0.2));
            Task.Delay(-1).Wait();
        }
    }
}
