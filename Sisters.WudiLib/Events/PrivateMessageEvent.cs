using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib.Events
{
    class PrivateMessageEvent : Event
    {
        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("sub_type")]
        public string SubType { get; set; }

        [JsonProperty("message_id")]
        public int MessageID { get; set; }

        [JsonProperty("user_id")]
        public int UserID { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("font")]
        public int Font { get; set; }
    }
}
