using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.WebSockets;

namespace Sisters.WudiLib.WebSocket.Reverse
{
    /// <summary>
    /// 反向 WebSocket 连接信息。
    /// </summary>
    internal class ReverseConnectionInfo : IDisposable
    {
        internal ReverseConnectionInfo(HttpListenerWebSocketContext webSocketContext, HttpListenerRequest request, long selfId, NegativeWebSocketEventListener listener)
        {
            SelfId = selfId;
            WebSocketManager = new NegativeWebSocketManager(webSocketContext.WebSocket);
            HttpApiClient = new CqHttpWebSocketApiClient(WebSocketManager);
            ApiPostListener = listener;
            ApiPostListener.ApiClient = HttpApiClient;
            ApiPostListener.SetManager(WebSocketManager);
            RequestHeaders = request.Headers;
            QueryString = request.QueryString;
            RemoteAddress = request.RemoteEndPoint.Address;
        }

        internal NegativeWebSocketManager WebSocketManager { get; set; }
        public long SelfId { get; }
        public NegativeWebSocketEventListener ApiPostListener { get; }
        public HttpApiClient HttpApiClient { get; }
        public NameValueCollection RequestHeaders { get; }
        public NameValueCollection QueryString { get; }
        public IPAddress RemoteAddress { get; }

        public void Dispose() => WebSocketManager?.Dispose();
    }
}
