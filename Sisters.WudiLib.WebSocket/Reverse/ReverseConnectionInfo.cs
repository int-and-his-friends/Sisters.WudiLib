using System;
using System.Net.WebSockets;

namespace Sisters.WudiLib.WebSocket.Reverse
{
    /// <summary>
    /// 反向 WebSocket 连接信息。
    /// </summary>
    public class ReverseConnectionInfo : IDisposable
    {
        internal ReverseConnectionInfo(HttpListenerWebSocketContext webSocketContext, long selfId, NegativeWebSocketEventListener listener)
        {
            SelfId = selfId;
            WebSocketManager = new NegativeWebSocketManager(webSocketContext.WebSocket);
            HttpApiClient = new CqHttpWebSocketApiClient(WebSocketManager);
            ApiPostListener = listener;
            ApiPostListener.ApiClient = HttpApiClient;
        }

        internal NegativeWebSocketManager WebSocketManager { get; set; }
        public long SelfId { get; }
        public NegativeWebSocketEventListener ApiPostListener { get; }
        public HttpApiClient HttpApiClient { get; }

        public void Dispose() => WebSocketManager?.Dispose();
    }
}
