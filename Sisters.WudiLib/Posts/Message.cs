using Newtonsoft.Json;
using System;

namespace Sisters.WudiLib.Posts
{
    public abstract class Message : Post
    {
        internal const string PrivateType = "private";
        internal const string GroupType = "group";
        internal const string DiscussType = "discuss";

        public Message()
            => messageLazy = new Lazy<ReceivedMessage>(() => new ReceivedMessage(_message));

        [JsonProperty("message_type")]
        public new string MessageType { get; private set; }
        [JsonProperty("message_id")]
        public int MessageId { get; private set; }
        [JsonProperty("user_id")]
        protected internal long UserId { get; private set; }
        [JsonProperty("message")]
        private object _message { get; set; }
        [JsonIgnore]
        Lazy<ReceivedMessage> messageLazy;
        [JsonIgnore]
        public ReceivedMessage Content => messageLazy.Value;
        //[JsonProperty("font")]
        //public int Font { get; private set; }
    }

    public class GroupMessage : Message
    {
        internal const string NormalType = "normal";
        internal const string AnonymousType = "anonymous";
        internal const string NoticeType = "notice";

        [JsonProperty("sub_type")]
        internal string SubType { get; private set; }
        [JsonProperty("group_id")]
        public long GroupId { get; private set; }
    }

    public class PrivateMessage : Message
    {
        public const string FriendType = "friend";
        public new const string GroupType = "group";
        public new const string DiscussType = "discuss";
        public const string OtherType = "other";

        [JsonProperty("sub_type")]
        public string SubType { get; private set; }
    }

    internal class AnonymousMessage : GroupMessage
    {
        [JsonProperty("anonymous")]
        internal string Anoymous { get; private set; }
        [JsonProperty("anonymous_flag")]
        internal string AnonymousFlag { get; set; }
    }

    public class DiscussMessage : Message
    {
        [JsonProperty("discuss_id")]
        protected long DiscussId { get; private set; }
    }
}
