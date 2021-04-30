using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Sisters.WudiLib.WebSocket
{
    internal abstract class WebSocketManager
    {
        private readonly SemaphoreSlim _sendSemaphore = new(1, 1);
        protected Task _listenTask;

        /// <summary>
        /// 指示当前是否已启动。若要检查当前是否可用，请使用 <see cref="IsAvailable"/> 属性。
        /// </summary>
        public virtual bool IsRunning => _listenTask?.IsCompleted == false;

        /// <summary>
        /// 获取当前 WebSocket 是否可用。注意自动重连过程中此项为
        /// <c>false</c>，但无法再次通过 <see cref="PositiveWebSocketManager.ConnectAsync(CancellationToken)"/>
        /// 连接。此外，在被动 WS 管理器中，此属性仅指示 WS 连接状态，不代表已经开始接收事件。
        /// </summary>
        public virtual bool IsAvailable => WebSocket?.State == WebSocketState.Open;

        internal Action<byte[]> OnMessage { get; set; }
        internal Action OnSocketDisconnected { get; set; }

        internal System.Net.WebSockets.WebSocket WebSocket { get; protected private set; }


        /// <summary>
        /// Send message through connected WebSocket.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="messageType"></param>
        /// <param name="endOfMessage"></param>
        /// <param name="cancellationToken">
        /// Will be saved when creating new instance of WebSocket.
        /// </param>
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

        protected abstract Task<System.Net.WebSockets.WebSocket> GetWebSocketAsync(CancellationToken cancellationToken);
        protected abstract Task RunListeningTask(CancellationToken cancellationToken);
    }
}