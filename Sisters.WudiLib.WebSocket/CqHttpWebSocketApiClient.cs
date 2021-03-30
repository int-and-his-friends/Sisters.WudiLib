using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Sisters.WudiLib.WebSocket.WebSocketUtility;

namespace Sisters.WudiLib.WebSocket
{
    public class CqHttpWebSocketApiClient : HttpApiClient, IDisposable
    {
        private static readonly JsonSerializerSettings s_jsonSerializerSettings = new JsonSerializerSettings();

        private static int GetRandomInt32()
        {
            Span<int> span = stackalloc int[1];
            System.Security.Cryptography.RandomNumberGenerator.Fill(
                System.Runtime.InteropServices.MemoryMarshal.AsBytes(span));
            return span[0];
        }

        protected CqHttpWebSocketApiClient() : base("http://wsdefault/")
        {
            _manager = new PositiveWebSocketManager(() => CreateUri(Uri, AccessToken))
            {
                OnSocketDisconnected = () =>
                {
                    var nSource = new CancellationTokenSource();
                    var oldSource = Interlocked.Exchange(ref _failedSource, nSource);
                    oldSource.Cancel();
                },
                OnMessage = bytes =>
                {
                    var s = Encoding.UTF8.GetString(bytes);
                    OnResponse(s);
                },
            };
        }

        public CqHttpWebSocketApiClient(string uri) : this() => Uri = uri;

        public CqHttpWebSocketApiClient(string uri, string accessToken)
            : this(uri) => AccessToken = accessToken;

        /// <summary>
        /// 获取 uri。
        /// </summary>
        public string Uri { get; }

        /// <summary>
        /// 获取 WebSocket 的连接地址。
        /// </summary>
        public override string ApiAddress
        {
            get => Uri;
            set => throw new InvalidOperationException("基于正向 WebSocket 的客户端无法在运行时改变连接地址。");
        }

        public override string AccessToken { get; set; }

        #region Echo
        private int _currentEcho = GetRandomInt32();
        protected int GetNextEcho() => Interlocked.Increment(ref _currentEcho);
        #endregion

        #region Call API and get response

        private TimeSpan _timeOut = TimeSpan.FromMinutes(1);
        private CancellationTokenSource _failedSource = new CancellationTokenSource();

        public TimeSpan TimeOut
        {
            get => _timeOut;
            set => _timeOut = value > TimeSpan.Zero || value == TimeSpan.FromMilliseconds(-1)
                    ? value
                    : throw new ArgumentOutOfRangeException(nameof(value), value, "TimeOut must be positive, or -1 millseconds.");
        }

        private readonly ConcurrentDictionary<int, WebSocketResponse> _responses = new();

        /// <summary>
        /// Note: swallows exceptions.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Success.</returns>
        protected virtual bool OnResponse(string response)
        {
            try
            {
                var r = JsonConvert.DeserializeObject<JObject>(response, s_jsonSerializerSettings);
                var echo = r["echo"].ToObject<int>();
                if (_responses.TryGetValue(echo, out var wsResponse))
                {
                    wsResponse.Data = r;
                    wsResponse.Lock.Release();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override async Task<JObject> CallRawJObjectAsync(string action, object data)
        {
            var jObject = new JObject();
            var echo = GetNextEcho();
            jObject["echo"] = echo;
            jObject["action"] = action;
            jObject["params"] = data is JObject j ? j : JObject.FromObject(data);
            var response = new WebSocketResponse
            {
                CancellationToken = _failedSource.Token,
            };
            using var l = response.Lock;
            if (!_responses.TryAdd(echo, response))
                throw new ApiAccessException("使用 WebSocket 访问 API 时出现并发错误。", null);
            try
            {
                await l.WaitAsync(TimeOut, response.CancellationToken).ConfigureAwait(false);
                return response.Data;
            }
            finally
            {
                _responses.TryRemove(echo, out _);
            }
        }
        #endregion

        #region Send request and manage WebSocket
        private readonly CancellationTokenSource _disposeSource = new();
        private readonly PositiveWebSocketManager _manager;

        protected override async Task<string> CallRawAsync(string action, string json)
        {
            var ws = await _manager.GetWebSocket(_disposeSource.Token).ConfigureAwait(false);
            await ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, _disposeSource.Token).ConfigureAwait(false);
            return string.Empty;
        }

        public void Dispose()
        {
            _disposeSource.Cancel();
            _manager.Dispose();
        }
        #endregion

        #region Embedded class
        private sealed class WebSocketResponse
        {
            public SemaphoreSlim Lock { get; } = new SemaphoreSlim(0, 1);
            public JObject Data { get; set; }
            public CancellationToken CancellationToken { get; set; }
        }
        #endregion
    }
}
