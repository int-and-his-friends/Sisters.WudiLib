using System;
using System.Threading;
using System.Threading.Tasks;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket.Test
{
    internal class Program
    {
        private static void ConfigListener(ApiPostListener cqWebSocketEvent)
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    Console.WriteLine("Available: {0}, Listening {1}", (cqWebSocketEvent as dynamic).IsAvailable, (cqWebSocketEvent as dynamic).IsListening);
                }
            });
            cqWebSocketEvent.MessageEvent += (api, e) =>
            {
                Console.WriteLine(e.Content.Raw);
                Console.WriteLine(api is null);
            };
            cqWebSocketEvent.FriendRequestEvent += (api, e) => true;
            cqWebSocketEvent.GroupInviteEvent += (api, e) => true;
            cqWebSocketEvent.AnonymousMessageEvent += (api, e) =>
            {
                Console.WriteLine("id|name|flag:{0}|{1}|{2}", e.Anonymous.Id, e.Anonymous.Name, e.Anonymous.Flag);
                api.BanMessageSource(e.GroupId, e.Source, 1);
            };
        }

        private static async Task TestPositive()
        {
            var cqWebSocketEvent = new CqHttpWebSocketEvent("ws://[::1]:6700/event", "");
            var httpApiClient = new CqHttpWebSocketApiClient("ws://[::1]:6700/api", "");
            cqWebSocketEvent.ApiClient = httpApiClient;
            ConfigListener(cqWebSocketEvent);

            await Task.Delay(TimeSpan.FromSeconds(3));
            var cancellationTokenSource = new CancellationTokenSource();
            await cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
            Console.ReadLine();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
            await Task.Delay(TimeSpan.FromSeconds(5));
            cancellationTokenSource.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            await cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
            await Task.Delay(-1);
        }

        private static async Task TestNegative()
        {
            var server = new Reverse.ReverseWebSocketServer("http://localhost:9191");
            server.ConfigureListener((l, _) => ConfigListener(l));

            var cancellationTokenSource = new CancellationTokenSource();
            server.Start(cancellationTokenSource.Token);
            Console.ReadLine();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
            await Task.Delay(TimeSpan.FromSeconds(5));
            cancellationTokenSource.Dispose();

            cancellationTokenSource = new CancellationTokenSource();
            server.Start(cancellationTokenSource.Token);
            await Task.Delay(-1);
        }

        private static Task Main(string[] args)
        {
            return TestNegative();
        }
    }
}
