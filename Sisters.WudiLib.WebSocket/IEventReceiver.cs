using System;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.WebSocket
{
    /// <summary>
    /// 表示实现了接收事件功能的接口。
    /// </summary>
    public interface IEventReceiver
    {
        /// <summary>
        /// 获取或设置事件处理器。
        /// </summary>
        Action<byte[], JObject> OnEvent { get; set; }
    }
}
