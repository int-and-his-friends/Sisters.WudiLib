using Newtonsoft.Json;

namespace Sisters.WudiLib.Api.Responses
{
    /// <summary>
    /// 表示发送消息的响应数据。
    /// </summary>
    public class MessageResponseData
    {
        /// <summary>
        /// 消息 ID。
        /// </summary>
        [JsonProperty("message_id")]
        public int MessageId { get; set; }
    }
}
