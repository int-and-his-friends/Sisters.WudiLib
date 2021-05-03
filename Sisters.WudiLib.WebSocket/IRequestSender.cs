using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.WebSocket
{
    /// <summary>
    /// 表示实现了发送请求和接收响应的接口。
    /// </summary>
    public interface IRequestSender
    {
        /// <summary>
        /// 收到响应时的处理器。
        /// </summary>
        Action<byte[], JObject> OnResponse { get; set; }
        /// <summary>
        /// Used for cleanup.
        /// </summary>
        Action OnSocketDisconnected { get; set; }

        /// <summary>
        /// 发送请求。
        /// </summary>
        /// <param name="buffer">请求内容。应为 JSON 字符串编码为 UTF-8 的数组。</param>
        /// <param name="cancellationToken">取消令牌。可能会被实现类保存以用于整个连接。</param>
        /// <returns>发送请求任务。</returns>
        Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default);
    }
}
