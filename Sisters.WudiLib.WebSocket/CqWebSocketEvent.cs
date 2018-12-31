using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket
{
    /// <summary>
    /// 事件上报的 WebSocket 客户端。请注意，WebSocket 客户端暂不支持直接通过返回值响应。
    /// </summary>
    public class CqWebSocketEvent : ApiPostListener
    {
        private readonly string _accessToken;
        private CancellationToken _cancellationToken;
        private readonly object _listenLock = new object();
        private Task _listenTask;

        /// <summary>
        /// 当前连接的 WebSocket 客户端。如果发生断线重连，则可能改变。
        /// </summary>
        protected System.Net.WebSockets.WebSocket WebSocket { get; private set; }

        /// <summary>
        /// 引发 <see cref="NotSupportedException"/>。
        /// </summary>
        /// <exception cref="NotSupportedException">不支持。</exception>
        public override string PostAddress
        {
            get => throw new NotSupportedException("WebSocket 不支持读取 PostAddress。");
            set => throw new NotSupportedException("WebSocket 不支持设置 PostAddress。");
        }

        /// <summary>
        /// 获取 uri。
        /// </summary>
        public string Uri { get; }

        /// <summary>
        /// 指示当前是否已启动监听。若要检查当前是否可用，请使用 <see cref="IsAvailable"/> 属性。
        /// </summary>
        public override bool IsListening => _listenTask?.IsCompleted == false;

        /// <summary>
        /// 获取当前是否能收到上报事件。注意自动重连过程中此项为 <c>false</c>，但无法再次通过 <see cref="StartListen()"/> 或 <see cref="StartListen(CancellationToken)"/> 连接。
        /// </summary>
        public virtual bool IsAvailable => WebSocket?.State == WebSocketState.Open;

        /// <summary>
        /// 构造通过 WebSocket 获取上报的监听客户端。
        /// </summary>
        /// <param name="uri">以 <c>ws://</c> 或者 <c>wss://</c> 开头的 uri，用于连接 WebSocket。</param>
        public CqWebSocketEvent(string uri) => Uri = uri;

        /// <summary>
        /// 构造通过 WebSocket 获取上报的监听客户端。
        /// </summary>
        /// <param name="uri">以 <c>ws://</c> 或者 <c>wss://</c> 开头的 uri，用于连接 WebSocket。</param>
        /// <param name="accessToken">Access Token.</param>
        public CqWebSocketEvent(string uri, string accessToken)
            : this(uri) => _accessToken = accessToken;

        /// <summary>
        /// 开始从 WebSocket 监听上报。
        /// </summary>
        /// <exception cref="Exception">连接失败等。</exception>
        public override void StartListen()
        {
            var cancellationToken = new CancellationToken();
            StartListen(cancellationToken);
        }

        /// <summary>
        /// 开始从 WebSocket 监听上报。
        /// </summary>
        /// <param name="cancellationToken">一个 <see cref="CancellationToken"/> 应该被使用，以通知此操作应被取消。</param>
        /// <exception cref="Exception">连接失败等。</exception>
        public void StartListen(CancellationToken cancellationToken)
        {
            lock (_listenLock)
            {
                if (WebSocket != null && !_cancellationToken.IsCancellationRequested)
                {
                    throw new InvalidOperationException("已经有监听任务在执行！");
                }
                _cancellationToken = cancellationToken;
                InitializeWebSocket(cancellationToken);
            }
            _listenTask = Listening(cancellationToken);
        }

        private async Task Listening(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];
            var ms = new MemoryStream();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string eventContent;
                byte[] eventArray; // 保留以供以后支持转发时计算签名。
                try
                {
                    var receiveResult = await WebSocket.ReceiveAsync(buffer, cancellationToken);
                    ms.Write(buffer, 0, receiveResult.Count);
                    if (!receiveResult.EndOfMessage) continue;
                    eventArray = ms.ToArray();
                    eventContent = Encoding.UTF8.GetString(eventArray);
                }
                catch (Exception)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        if (WebSocket?.State != WebSocketState.Open)
                        {
                            (WebSocket as IDisposable).Dispose();
                            InitializeWebSocket(cancellationToken);
                        }
                    }
                    catch (Exception)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    ms = new MemoryStream();
                    continue;
                }

                ms = new MemoryStream();
                _ = Task.Run(() =>
                {
                    ForwardAsync(eventContent, null);
                    if (string.IsNullOrEmpty(eventContent))
                        return;

                    try
                    {
                        ProcessPost(eventContent);
                    }
                    catch (Exception e)
                    {
                        LogException(e, eventContent);
                    }
                });
            }
        }

        private void InitializeWebSocket(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Uri uri = CreateUri(Uri, _accessToken);
            ClientWebSocket clientWebSocket = CreateWebSocket(uri, cancellationToken).GetAwaiter().GetResult();
            WebSocket = clientWebSocket;
        }

        private static Uri CreateUri(string url, string accessToken)
        {
            var uriBuilder = new UriBuilder(url);
            if (!string.IsNullOrEmpty(accessToken))
            {
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["access_token"] = accessToken;
                /* 在.NET Framework 中，这里的 ToString 会编码成 %uxxxx 的格式，
                 * 现在已经不用这种格式了。.NET Core 可以正确处理。解决这个问题之后，
                 * 此类库应该可以用于 .NET Framework。
                 */
                uriBuilder.Query = query.ToString();
            }
            Uri uri = uriBuilder.Uri;
            return uri;
        }

        private static async Task<ClientWebSocket> CreateWebSocket(Uri uri, CancellationToken cancellationToken)
        {
            var clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(uri, cancellationToken);
            return clientWebSocket;
        }
    }
}
