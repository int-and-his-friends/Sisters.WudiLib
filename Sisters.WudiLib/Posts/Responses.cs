using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    public abstract class Response
    {
        [JsonProperty("block")]
        public bool Block { get; set; }
    }

    public class GroupRequestResponse : Response
    {
        [JsonProperty("approve")]
        public bool Approve { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    public class FriendRequestResponse : Response
    {
        [JsonProperty("approve")]
        public bool Approve { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }

    public class MessageResponse : Response
    {
        public WudiLib.Message Reply { get; set; }

        [JsonProperty("reply")]
        private object _reply => Reply?.Serializing;
    }

    public class MultiMessageResponse : MessageResponse
    {
        [JsonProperty("at_sender")]
        public bool AtSender { get; set; }
    }

    public class GroupMessageResponse : MultiMessageResponse
    {
        [JsonProperty("delete")]
        public bool Recall { get; set; }

        [JsonProperty("kick")]
        public bool Kick { get; set; }

        [JsonProperty("ban")]
        public bool Ban { get; set; }
    }
}
