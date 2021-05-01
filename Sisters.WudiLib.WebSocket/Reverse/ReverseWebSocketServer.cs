using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket.Reverse
{
    public sealed class ReverseWebSocketServer
    {
        private readonly HttpListener _httpListener = new();

        private Func<HttpListenerRequest, Task<bool>> _authentication = _ => Task.FromResult(true);
        private Func<long, NegativeWebSocketEventListener> _createListener = _ => new NegativeWebSocketEventListener();

        public ReverseWebSocketServer(int port)
        {
            if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port), "Port 必须是 0-65535 的数。");
            _httpListener.Prefixes.Add($"http://+:{port}");
        }

        public ReverseWebSocketServer(string prefix)
        {
            _httpListener.Prefixes.Add(prefix);
        }

        ///// <summary>
        ///// 返回默认的 API 客户端。当调用 API
        ///// 时，客户端任选一个当前已建立的 WebSocket 连接发送请求。
        ///// </summary>
        //public HttpApiClient DefaultClient { get; }

        ///// <summary>
        ///// 返回默认的 Listener。此 Listener 会处理来自所有连接的请求。
        ///// </summary>
        //public ApiPostListener DefaultListener { get; }

        private async Task StartListenAsync()
        {
            _httpListener.Start();
            while (true)
            {
                var context = await _httpListener.GetContextAsync().ConfigureAwait(false);
                _ = Process(context);
            }
        }

        private async Task Process(HttpListenerContext context)
        {
            // Check ws request
            if (!context.Request.IsWebSocketRequest)
            {
                using var response = context.Response;
                await RefuseBadRequest(response, "Must use WebSocket connection.").ConfigureAwait(false);
                return;
            }

            // Check necessary headers
            if (context.Request.Headers["X-Client-Role"] != "Universal")
            {
                using var response = context.Response;
                await RefuseBadRequest(response, "Must use Universal connection.").ConfigureAwait(false);
                return;
            }
            if (!long.TryParse(context.Request.Headers["X-Self-ID"], out var selfId))
            {
                using var response = context.Response;
                await RefuseBadRequest(response, "X-Self-ID header is not found or not valid.").ConfigureAwait(false);
                return;
            }

            var authPass = await _authentication(context.Request).ConfigureAwait(false);
            if (!authPass)
            {
                using var response = context.Response;
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            var wsContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
            var info = new ReverseConnectionInfo(wsContext, selfId, _createListener(selfId));
            info.WebSocketManager.Start(default);

            // TODO: 把建立的连接存起来
            // TODO: 检测连接是否断开。当断开时回收资源，并从连接列表中清除。
        }

        private static async Task RefuseBadRequest(HttpListenerResponse response, string responseText)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.ContentType = "text/plain";
            using var writer = new StreamWriter(response.OutputStream);
            await writer.WriteLineAsync(responseText).ConfigureAwait(false);
        }

        public void Start() => _ = StartListenAsync();

        public void SetAuthentication(Func<HttpListenerRequest, Task<bool>> authenticationFunction)
        {
            _authentication = authenticationFunction;
        }

        /// <summary>
        /// 根据 Access Token 和连接的 QQ 号验证。
        /// </summary>
        /// <param name="accessToken">Access Token，如果为 <c>null</c>，则跳过此认证。注意仅验证 QQ 号并不安全，因为请求可能是伪造的。</param>
        /// <param name="selfId">连接的 QQ 号，如果为 <c>null</c>，则跳过 QQ 号验证。注意仅验证 QQ 号并不安全，因为请求可能是伪造的。</param>
        public void SetAuthenticationFromAccessTokenAndUserId(string accessToken, long? selfId)
        {
            _authentication = r =>
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    var headValue = r.Headers["Authorization"];
                    if (string.IsNullOrWhiteSpace(headValue))
                        return Task.FromResult(false);
                    int spaceIndex = headValue.IndexOf(' ');
                    if (!headValue.AsSpan().Slice(spaceIndex + 1).SequenceEqual(accessToken))
                    {
                        return Task.FromResult(false);
                    }
                }
                if (selfId != null)
                {
                    var idString = selfId.ToString();
                    var selfIdValue = r.Headers["X-Self-ID"];
                    if (selfIdValue != idString)
                        return Task.FromResult(false);
                }
                return Task.FromResult(true);
            };
        }

        /// <summary>
        /// 配置 Listener。
        /// </summary>
        /// <param name="config">配置委托。第二个参数是自己的 QQ 号。</param>
        public void ConfigureListener(Action<ApiPostListener, long> config)
        {
            ConfigureListener<NegativeWebSocketEventListener>(config);
        }

        /// <summary>
        /// 用派生类配置 Listener。
        /// </summary>
        /// <typeparam name="T">派生的类。</typeparam>
        /// <param name="config">配置委托。第二个参数是自己的 QQ 号。</param>
        public void ConfigureListener<T>(Action<ApiPostListener, long> config) where T : NegativeWebSocketEventListener, new()
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _createListener = selfId =>
            {
                var result = new T();
                config(result, selfId);
                return result;
            };
        }
    }
}
