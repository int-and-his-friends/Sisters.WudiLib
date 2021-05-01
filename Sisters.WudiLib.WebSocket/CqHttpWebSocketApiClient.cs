using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Sisters.WudiLib.WebSocket.WebSocketUtility;

namespace Sisters.WudiLib.WebSocket
{
    /// <summary>
    /// 实现通过正向 WebSocket 访问 OneBot API 的类。
    /// </summary>
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

        /// <summary>
        /// 初始化实例，可以被子类调用。
        /// </summary>
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
                OnResponse = (_, jObject) => OnResponse(jObject),
                AutoReconnect = false,
            };
        }

        /// <summary>
        /// 从给定 WebSocket URL 创建实例。
        /// </summary>
        /// <param name="uri">正向 WS 监听地址（以 ws:// 或 wss:// 开头）。</param>
        public CqHttpWebSocketApiClient(string uri) : this() => Uri = uri;

        /// <summary>
        /// 从给定 WebSocket URL 和 Access Token 创建实例。
        /// </summary>
        /// <param name="uri">正向 WS 监听地址（以 ws:// 或 wss:// 开头）。</param>
        /// <param name="accessToken">Access Token。</param>
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

        /// <summary>
        /// 获取或设置 AccessToken。将在下次自动重连时生效。
        /// </summary>
        public override string AccessToken { get; set; }

        #region Echo
        private int _currentEcho = GetRandomInt32();
        /// <summary>
        /// 获取下一个可用的 Echo 值。
        /// </summary>
        /// <returns></returns>
        protected int GetNextEcho() => Interlocked.Increment(ref _currentEcho);
        #endregion

        #region Call API and get response

        private TimeSpan _timeOut = TimeSpan.FromMinutes(1);
        private CancellationTokenSource _failedSource = new CancellationTokenSource();

        /// <summary>
        /// 等待 API 响应的超时值。
        /// </summary>
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
        protected virtual bool OnResponse(JObject r)
        {
            try
            {
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
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 用未包装的方式调用 API。
        /// </summary>
        /// <param name="action">操作。</param>
        /// <param name="data">参数数据。</param>
        /// <returns>响应结果。</returns>
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
                await CallRawAsync(action, jObject.ToString(Formatting.None)).ConfigureAwait(false);
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
        private readonly WebSocketManager _manager;

        /// <summary>
        /// 发送调用消息，被 <see cref="CallRawJObjectAsync(string, object)"/> 调用
        /// </summary>
        /// <param name="action">操作。</param>
        /// <param name="json">参数 json。</param>
        /// <returns>由于无法直接获取响应，始终为空字符串。</returns>
        protected override async Task<string> CallRawAsync(string action, string json)
        {
            await _manager.SendAsync(Encoding.UTF8.GetBytes(json), _disposeSource.Token).ConfigureAwait(false);
            return string.Empty;
        }

        /// <summary>
        /// 析构此对象。
        /// </summary>
        public void Dispose()
        {
            _disposeSource.Cancel();
            (_manager as IDisposable)?.Dispose();
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
