using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Sisters.WudiLib.WebSocket.WebSocketUtility;

namespace Sisters.WudiLib.WebSocket
{
    internal class PositiveWebSocketManager : IDisposable
    {
        private CancellationToken _cancellationToken;
        private readonly SemaphoreSlim _connectSemaphore = new(1, 1);
        private readonly SemaphoreSlim _sendSemaphore = new(1, 1);
        private Task _listenTask;

        private Func<Uri> _getUri;

        internal Func<Uri> GetUri
        {
            get => _getUri;
            set => _getUri = value ?? throw new ArgumentNullException(nameof(value));
        }

        public PositiveWebSocketManager(Func<Uri> getUri)
            => GetUri = getUri;

        public PositiveWebSocketManager(string url, string accessToken)
            : this(() => CreateUri(url, accessToken))
        { }

        internal Action<byte[]> OnMessage { get; set; }
        internal Action OnSocketDisconnected { get; set; }
        internal bool AutoReconnect { get; set; } = true;

        internal System.Net.WebSockets.WebSocket WebSocket { get; private set; }

        /// <summary>
        /// 指示当前是否已启动。若要检查当前是否可用，请使用 <see cref="IsAvailable"/> 属性。
        /// </summary>
        public bool IsRunning => _listenTask?.IsCompleted == false;

        /// <summary>
        /// 获取当前 WebSocket 是否可用。注意自动重连过程中此项为
        /// <c>false</c>，但无法再次通过 <see cref="ConnectAsync(CancellationToken)"/> 连接。
        /// </summary>
        public bool IsAvailable => WebSocket?.State == WebSocketState.Open;

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await _connectSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (WebSocket != null && !_cancellationToken.IsCancellationRequested)
                {
                    throw new InvalidOperationException("已经有监听任务在执行！");
                }
                _cancellationToken = cancellationToken;
                await InitializeWebSocketAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _connectSemaphore.Release();
            }
            _listenTask = Listening(cancellationToken);
        }

        /// <summary>
        /// Trys to connect to WS server. Does not throw an exception if has connected.
        /// Still throws if connection fails.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task<System.Net.WebSockets.WebSocket> GetWebSocket(CancellationToken cancellationToken)
        {
            if (WebSocket != null && !_cancellationToken.IsCancellationRequested)
            {// ignore
                return WebSocket;
            }
            System.Net.WebSockets.WebSocket ret;
            await _connectSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (WebSocket != null && !_cancellationToken.IsCancellationRequested)
                {// ignore
                    return WebSocket;
                }
                _cancellationToken = cancellationToken;
                ret = await InitializeWebSocketAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _connectSemaphore.Release();
            }
            _listenTask = Listening(cancellationToken);
            return ret;
        }

        private async Task Listening(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];
            var ms = new MemoryStream();
            while (true)
            {
                ThrowIfCanceledOrDisposed(cancellationToken);
                byte[] eventArray;
                try
                {
                    var receiveResult = await WebSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                    ms.Write(buffer, 0, receiveResult.Count);
                    if (!receiveResult.EndOfMessage) continue;
                    eventArray = ms.ToArray();
                }
                catch (Exception)
                {
                    if (!AutoReconnect && !IsAvailable)
                    {
                        var ws = WebSocket;
                        WebSocket = null;
                        (ws as IDisposable)?.Dispose();
                        OnSocketDisconnected?.Invoke();
                        break;
                    }
                    await ReconnectIfNecessaryAsync(cancellationToken).ConfigureAwait(false);
                    ms = new MemoryStream();
                    continue;
                }

                try
                {
                    OnMessage?.Invoke(eventArray);
                }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
                catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
                {
                    // ignored
                }
                ms = new MemoryStream();
            }
        }

        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            await _sendSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var ws = await GetWebSocket(cancellationToken).ConfigureAwait(false);
                await ws.SendAsync(buffer, messageType, endOfMessage, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _sendSemaphore.Release();
            }
        }

        #region management
        private async Task<ClientWebSocket> InitializeWebSocketAsync(CancellationToken cancellationToken)
        {
            ThrowIfCanceledOrDisposed(cancellationToken);
            ClientWebSocket clientWebSocket = await CreateWebSocket(GetUri(), cancellationToken).ConfigureAwait(false);
            WebSocket = clientWebSocket;
            return clientWebSocket;
        }

        private async Task ReconnectIfNecessaryAsync(CancellationToken cancellationToken)
        {
            ThrowIfCanceledOrDisposed(cancellationToken);
            try
            {
                if (WebSocket?.State != WebSocketState.Open)
                {
                    (WebSocket as IDisposable)?.Dispose();
                    await InitializeWebSocketAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                ThrowIfCanceledOrDisposed(cancellationToken);
            }
        }
        #endregion

        #region utils
        private void ThrowIfCanceledOrDisposed(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_disposedValue)
                throw new ObjectDisposedException(nameof(PositiveWebSocketManager), "此对象已被 dispose。");
        }
        #endregion

        #region dispose
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
                if (disposing)
                {
                    (WebSocket as IDisposable)?.Dispose();
                }
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
