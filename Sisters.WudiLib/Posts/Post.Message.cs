using System;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Message : Post
    {
        /// <summary>
        /// 表示此消息为私聊消息。
        /// </summary>
        public const string PrivateType = "private";
        /// <summary>
        /// 表示此消息为群聊消息。
        /// </summary>
        public const string GroupType = "group";
        /// <summary>
        /// 表示此消息为讨论组消息。
        /// </summary>
        public const string DiscussType = "discuss";

        internal new const string TypeField = "message_type";

        public Message()
            => _messageLazy = new Lazy<ReceivedMessage>(() => new ReceivedMessage(ObjMessage));

        /// <summary>
        /// 消息类型（群、私聊、讨论组）。不建议使用本属性判断类型，请使用 <c>is</c> 运算符进行判断。
        /// </summary>
        [JsonProperty(TypeField)]
        public string MessageType { get; private set; }
        [JsonProperty("message_id")]
        public int MessageId { get; private set; }
        [JsonProperty("message")]
        private object ObjMessage { get; set; }
        [JsonIgnore]
        private readonly Lazy<ReceivedMessage> _messageLazy;
        [JsonIgnore]
        public ReceivedMessage Content => _messageLazy.Value;
        [JsonProperty("raw_message")]
        public string RawMessage { get; private set; }
        [JsonProperty("font")]
        public int Font { get; private set; }

        public abstract override Endpoint Endpoint { get; }
        public virtual MessageSource Source => new MessageSource(UserId);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GroupMessage : Message
    {
        internal const string NormalType = "normal";
        internal const string AnonymousType = "anonymous";
        internal const string NoticeType = "notice";

        [JsonProperty(SubTypeField)]
        internal string SubType { get; private set; }
        [JsonProperty("group_id")]
        public long GroupId { get; private set; }

        public override Endpoint Endpoint => new GroupEndpoint(GroupId);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PrivateMessage : Message
    {
        public const string FriendType = "friend";
        public new const string GroupType = "group";
        public new const string DiscussType = "discuss";
        public const string OtherType = "other";

        [JsonProperty(SubTypeField)]
        public string SubType { get; private set; }

        public override Endpoint Endpoint => new PrivateEndpoint(UserId);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class AnonymousMessage : GroupMessage
    {
        [JsonProperty("anonymous")]
        internal AnonymousInfo Anonymous { get; private set; }

        public override MessageSource Source => new MessageSource(UserId, Anonymous.Flag, Anonymous.Name, true);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DiscussMessage : Message
    {
        [JsonProperty("discuss_id")]
        internal long DiscussId { get; private set; }

        public override Endpoint Endpoint => new DiscussEndpoint(DiscussId);
    }
}
