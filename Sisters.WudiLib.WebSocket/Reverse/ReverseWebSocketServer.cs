using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket.Reverse
{
    /// <summary>
    /// 反向 WebSocket 服务器。
    /// </summary>
    public sealed class ReverseWebSocketServer
    {
        private HttpListener _httpListener = new();
        private readonly SemaphoreSlim _listeningSemaphore = new SemaphoreSlim(1, 1);

        private Func<HttpListenerRequest, Task<bool>> _authentication = _ => Task.FromResult(true);
        private Func<long, NegativeWebSocketEventListener> _createListener = _ => new NegativeWebSocketEventListener();
        private readonly Action<HttpListener> _configHttpListener;

        /// <summary>
        /// 通过端口号初始化反向 WebSocket 服务器。
        /// </summary>
        /// <param name="port">端口号。</param>
        /// <exception cref="ArgumentOutOfRangeException">端口号不合法。</exception>
        public ReverseWebSocketServer(int port)
        {
            if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port), "Port 必须是 0-65535 的数。");
            _configHttpListener = httpListener => httpListener.Prefixes.Add($"http://+:{port}/");
            _configHttpListener(_httpListener);
        }

        /// <summary>
        /// 通过前缀初始化反向 WebSocket 服务器。
        /// </summary>
        /// <param name="prefix">监听前缀。</param>
        /// <exception cref="ArgumentNullException"><c>prefix</c> 为 <c>null</c>。</exception>
        /// <exception cref="UriFormatException">传入的不是合法的 URI 格式。</exception>
        /// <exception cref="ArgumentException">传入的前缀不合法。</exception>
        public ReverseWebSocketServer(string prefix)
        {
            var uriBuilder = new UriBuilder(prefix);
            if ("ws".Equals(uriBuilder.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                uriBuilder.Scheme = "http";
            }
            else if ("wss".Equals(uriBuilder.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                uriBuilder.Scheme = "https";
            }
            prefix = uriBuilder.Uri.AbsoluteUri;
            if (!prefix.EndsWith('/'))
            {
                prefix += "/";
            }
            _configHttpListener = httpListener => httpListener.Prefixes.Add(prefix);
            _configHttpListener(_httpListener);
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

        private Task RunListeningTask(CancellationToken cancellationToken)
        {
            // 此方法返回 Task，但是不能标记为 async。
            // 如果标记为 async，抛出的异常将被包裹在 Task 中，而不会直接被抛出。
            if (!_listeningSemaphore.Wait(0))
            {
                throw new InvalidOperationException("反向 WebSocket 服务器已经在监听中。");
            }
            return RunInternalAsync(cancellationToken);

            async Task RunInternalAsync(CancellationToken cancellationToken)
            {
                try
                {
                    if (_httpListener is null)
                    {
                        // 为了检查前缀格式，此类的构造函数中会默认初始化一个 HttpListener。
                        // 如果检测到已初始化，则直接使用。
                        // 否则，重新初始化 HttpListener。
                        _httpListener = new HttpListener();
                        _configHttpListener(_httpListener);
                    }
                    _httpListener.Start();
                    cancellationToken.Register(() => _httpListener.Stop());
                    while (true)
                    {
                        var context = await _httpListener.GetContextAsync().ConfigureAwait(false);
                        _ = Process(context, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    var oldListener = Interlocked.Exchange(ref _httpListener, null);
                    _listeningSemaphore.Release();
                    (oldListener as IDisposable)?.Dispose();
                }
            }
        }

        private async Task Process(HttpListenerContext context, CancellationToken cancellationToken)
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
            info.WebSocketManager.Start(cancellationToken);

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

        /// <summary>
        /// 开始监听反向 WebSocket 请求。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <exception cref="InvalidOperationException">正在监听，不能重复启动。</exception>
        public void Start(CancellationToken cancellationToken = default) => _ = RunListeningTask(cancellationToken);

        /// <summary>
        /// 设置手动鉴权。
        /// </summary>
        /// <param name="authenticationFunction"></param>
        public void SetAuthentication(Func<HttpListenerRequest, Task<bool>> authenticationFunction)
        {
            _authentication = authenticationFunction;
        }

        /// <summary>
        /// 根据 Access Token 和连接的 QQ 号鉴权验证。
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
