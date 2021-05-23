using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket
{
    /// <summary>
    /// 事件上报的 WebSocket 客户端。请注意，WebSocket 客户端暂不支持直接通过返回值响应。
    /// </summary>
    public class CqHttpWebSocketEvent : ApiPostListener, IDisposable
    {
        /// <summary>
        /// 当前连接的 WebSocket 客户端。如果发生断线重连，则可能改变。
        /// </summary>
        protected System.Net.WebSockets.WebSocket WebSocket => _manager.WebSocket;

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
        public override bool IsListening => _manager.IsRunning;

        /// <summary>
        /// 获取当前是否能收到上报事件。注意自动重连过程中此项为 <c>false</c>，但无法再次通过 <see cref="StartListen()"/> 或 <see cref="StartListen(CancellationToken)"/> 连接。
        /// </summary>
        public virtual bool IsAvailable => _manager.IsRunning && _manager.IsAvailable;

        /// <summary>
        /// 构造通过 WebSocket 获取上报的监听客户端。
        /// </summary>
        /// <param name="uri">以 <c>ws://</c> 或者 <c>wss://</c> 开头的 uri，用于连接 WebSocket。</param>
        public CqHttpWebSocketEvent(string uri) : this(uri, string.Empty)
        { }

        /// <summary>
        /// 构造通过 WebSocket 获取上报的监听客户端。
        /// </summary>
        /// <param name="uri">以 <c>ws://</c> 或者 <c>wss://</c> 开头的 uri，用于连接 WebSocket。</param>
        /// <param name="accessToken">Access Token.</param>
        public CqHttpWebSocketEvent(string uri, string accessToken)
        {
            _manager = new PositiveWebSocketManager(uri, accessToken)
            {
                OnEvent = (bytes, jObject) => Task.Run(() => OnEventAsync(bytes, jObject)),
                AutoReconnect = true,
            };
        }

        /// <summary>
        /// 开始从 WebSocket 监听上报。
        /// </summary>
        /// <exception cref="Exception">连接失败等。</exception>
        public override void StartListen()
            => _ = StartListen(default(CancellationToken));

        /// <summary>
        /// 开始从 WebSocket 监听上报。
        /// </summary>
        /// <param name="cancellationToken">一个 <see cref="CancellationToken"/> 应该被使用，以通知此操作应被取消。</param>
        /// <exception cref="Exception">连接失败等。</exception>
        public async Task StartListen(CancellationToken cancellationToken)
            => await _manager.ConnectAsync(cancellationToken).ConfigureAwait(false);

        private async Task OnEventAsync(byte[] eventArray, JObject eventObject)
        {
            ForwardAsync(eventArray, Encoding.UTF8, null);

            try
            {
                await this.ProcessWSMessageAsync(eventObject).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogException(e, Encoding.UTF8.GetString(eventArray));
            }
        }

        #region Manage WebSocket
        private readonly CancellationTokenSource _disposeSource = new();
        private readonly PositiveWebSocketManager _manager;

        /// <summary>
        /// Disconnects from remote and disposes this object.
        /// </summary>
        public void Dispose()
        {
            _disposeSource.Cancel();
            _manager.Dispose();
        }
        #endregion
    }
}
