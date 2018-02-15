using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib.Responses
{
    public class SendMessageResponseData
    {
        [JsonProperty("message_id")]
        public long MessageId { get; internal set; }
    }
}
