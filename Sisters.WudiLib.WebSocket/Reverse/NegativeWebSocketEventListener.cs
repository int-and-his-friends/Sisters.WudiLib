using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket.Reverse
{
    /// <summary>
    /// 反向 WebSocket 连接的事件监听器。
    /// </summary>
    public class NegativeWebSocketEventListener : ApiPostListener
    {
        private NegativeWebSocketManager _negativeWebSocketManager;
        private bool _isStarted;
        private BlockingCollection<(byte[] data, JObject jObject)> _backlogEventBag = new(new ConcurrentQueue<(byte[], JObject)>());

        internal void SetManager(NegativeWebSocketManager negativeWebSocketManager)
        {
            negativeWebSocketManager.OnEvent = (data, jObject) =>
            {
                try
                {
                    // TryAdd may still throw.
                    // Hence, try-catch block is necessary.
                    _backlogEventBag.Add((data, jObject));
                }
                catch (Exception)
                {
                    // When it throws, the listener must have been started.
                    _ = OnEventAsync(data, jObject);
                }
            };
            _negativeWebSocketManager = negativeWebSocketManager;
            _negativeWebSocketManager.SocketDisconnected += () => SocketDisconnected?.Invoke();
        }

        private async Task OnEventAsync(byte[] data, JObject jObject)
        {
            ForwardAsync(data, Encoding.UTF8, null);
            ProcessPost(jObject);
            try
            {
                await this.ProcessWSMessageAsync(jObject).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogException(e, Encoding.UTF8.GetString(data));
            }
        }

        /// <summary>
        /// 当反向 WebSocket 连接断开时触发。
        /// </summary>
        public event Action SocketDisconnected;

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
        public sealed override bool IsListening => _isStarted && _negativeWebSocketManager?.IsRunning == true;

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

        internal void StartProcessEventInternal()
        {
            _isStarted = true;
            _negativeWebSocketManager.OnEvent = (bytes, jObject) => _ = OnEventAsync(bytes, jObject);
            _backlogEventBag.CompleteAdding();
            using var bag = _backlogEventBag;
            while (bag.TryTake(out var tuple))
            {
                var (data, jObject) = tuple;
                _ = OnEventAsync(data, jObject);
            }
            _backlogEventBag = null;
        }
    }
}
