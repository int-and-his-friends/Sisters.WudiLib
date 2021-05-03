using System;
using System.Text;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket.Reverse
{
    /// <summary>
    /// 反向 WebSocket 连接的事件监听器。
    /// </summary>
    public class NegativeWebSocketEventListener : ApiPostListener
    {
        private NegativeWebSocketManager _negativeWebSocketManager;

        internal void SetManager(NegativeWebSocketManager negativeWebSocketManager)
        {
            negativeWebSocketManager.OnEvent = (data, jObject) =>
            {
                ForwardAsync(data, Encoding.UTF8, null);
                ProcessPost(jObject);
            };
            _negativeWebSocketManager = negativeWebSocketManager;
        }

        /// <summary>
        /// 引发 <see cref="NotSupportedException"/>。
        /// </summary>
        /// <exception cref="NotSupportedException">不支持。</exception>
        public sealed override string PostAddress
        {
            get => throw new NotSupportedException("WebSocket 不支持读取 PostAddress。");
            set => throw new NotSupportedException("WebSocket 不支持设置 PostAddress。");
        }

        /// <summary>
        /// 指示当前是否已启动监听。若要检查当前是否可用，请使用 <see cref="IsAvailable"/> 属性。
        /// </summary>
        public sealed override bool IsListening => _negativeWebSocketManager?.IsRunning == true;

        /// <summary>
        /// 指示当前是否还可以收到事件上报。
        /// </summary>
        public bool IsAvailable => IsListening && _negativeWebSocketManager?.IsAvailable == true;

        /// <summary>
        /// 引发 <see cref="NotSupportedException"/>。
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public sealed override void StartListen()
            => throw new NotSupportedException("反向 WebSocket 在建立时即已开始监听，无法手动设置。");
    }
}
