using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.WebSocket
{
    internal class CqHttpApiWebSocketClient : HttpApiClient
    {
        private static readonly object s_randomLock = new object();
        private static readonly Random s_random = new Random();

        private static int GetRandomInt32()
        {
            lock (s_randomLock)
            {
                return s_random.Next();
            }
        }

        public virtual async Task ConnectAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var ws = new ClientWebSocket();
                var source = new CancellationTokenSource();
                await ws.ConnectAsync(new Uri("wss://api.bleatingsheep.org/ws"), source.Token);
            }
        }

        private int _currentEcho = GetRandomInt32();
        protected int GetNextEcho() => Interlocked.Increment(ref _currentEcho);

        protected override Task<JObject> CallRawJObjectAsync(string action, object data)
        {
            var jObject = JObject.FromObject(data);
            var echo = GetNextEcho();
            jObject["echo"] = echo;
            return base.CallRawJObjectAsync(action, data);
        }
    }
}
