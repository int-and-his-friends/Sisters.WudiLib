using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib.Responses
{
    /// <summary>
    /// 发送消息后返回的数据
    /// </summary>
    public class SendMessageResponseData
    {
        internal SendMessageResponseData()
        {

        }

        /// <summary>
        /// 消息 ID
        /// </summary>
        [JsonProperty("message_id")]
        public long MessageId { get; internal set; }
    }
}
