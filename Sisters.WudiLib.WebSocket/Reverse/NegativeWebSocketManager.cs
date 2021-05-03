using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sisters.WudiLib.WebSocket.Reverse
{
    /// <summary>
    /// 处理单个 WebSocket 连接的被动连接管理器。
    /// </summary>
    internal class NegativeWebSocketManager : WebSocketManager, IDisposable
    {
        private readonly object _startLock = new object();

        public NegativeWebSocketManager(System.Net.WebSockets.WebSocket webSocket)
            => WebSocket = webSocket;

        /// <summary>
        /// 开始处理反向 WS 连接消息。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <exception cref="InvalidOperationException">已经开始执行，无法再次开始。</exception>
        public void Start(CancellationToken cancellationToken)
        {
            lock (_startLock)
            {
                _listenTask = _listenTask is null
                    ? RunListeningTask(cancellationToken)
                    : throw new InvalidOperationException("已经开始执行。");
            }
        }

        protected override Task<System.Net.WebSockets.WebSocket> GetWebSocketAsync(CancellationToken cancellationToken)
            => Task.FromResult(WebSocket);

        protected async override Task RunListeningTask(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];
            var ms = new MemoryStream();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                byte[] eventArray;
                try
                {
                    var receiveResult = await WebSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                    ms.Write(buffer, 0, receiveResult.Count);
                    if (!receiveResult.EndOfMessage)
                        continue;
                    eventArray = ms.ToArray();
                }
                catch (Exception)
                {
                    if (!IsAvailable)
                    {
                        // 当出现异常后确认了不可用，被动管理无法重连。
                        // 回收资源，然后退出。
                        (WebSocket as IDisposable)?.Dispose();
                        OnSocketDisconnected?.Invoke();
                        break;
                    }
                    ms = new MemoryStream();
                    continue;
                }

                try
                {
                    Dispatch(eventArray);
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

        public void Dispose() => WebSocket?.Dispose();
    }
}
