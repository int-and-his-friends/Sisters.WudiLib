﻿using System;
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
            _listenTask = RunListeningTask(cancellationToken);
        }

        /// <summary>
        /// Trys to connect to WS server. Does not throw an exception if has connected.
        /// Still throws if connection fails.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        private async Task<System.Net.WebSockets.WebSocket> GetWebSocketAsync(CancellationToken cancellationToken)
        {
            // 除了此方法和上面的 ConnectAsync 方法，还有 ReconnectIfNecessaryAsync
            // 方法也调用了 InitializeWebSocketAsync。但是 ReconnectIfNecessaryAsync
            // 没有使用 _connectSemaphore 控制并发。这是因为 ReconnectIfNecessaryAsync 只会在重连时调用，
            // 而重连全过程不会满足此处的进入条件，故不会破坏线程安全。
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
            _listenTask = RunListeningTask(cancellationToken);
            return ret;
        }

        private async Task RunListeningTask(CancellationToken cancellationToken)
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
                        // 当出现异常后确认了不可用，并且不需要自动重连时，回收资源，
                        // 然后退出。
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
                var ws = await GetWebSocketAsync(cancellationToken).ConfigureAwait(false);
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
            ClientWebSocket clientWebSocket = await CreateWebSocketAsync(GetUri(), cancellationToken).ConfigureAwait(false);
            WebSocket = clientWebSocket;
            return clientWebSocket;
        }

        private async Task ReconnectIfNecessaryAsync(CancellationToken cancellationToken)
        {
            ThrowIfCanceledOrDisposed(cancellationToken);
            try
            {
                if (!IsAvailable)
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

        internal static async Task<ClientWebSocket> CreateWebSocketAsync(Uri uri, CancellationToken cancellationToken)
        {
            var clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
            return clientWebSocket;
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
