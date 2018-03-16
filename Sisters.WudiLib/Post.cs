using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Sisters.WudiLib
{
    public class Post
    {
        internal const string MessageType = "message";
        internal const string EventType = "event";
        internal const string RequestType = "request";

        internal Post() { }

        [JsonProperty("post_type")]
        internal string PostType { get; private set; }

        [JsonProperty("time")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset Time { get; private set; }
        [JsonProperty("self_id")]
        public int SelfId { get; private set; }
    }

    public abstract class Request : Post
    {
        internal const string FriendType = "friend";
        internal const string GroupType = "group";

        internal Request() { }

        [JsonProperty("request_type")]
        internal new string RequestType { get; private set; }

        [JsonProperty("flag")]
        public string Flag { get; private set; }
        [JsonProperty("message")]
        public string Message { get; private set; }
        [JsonProperty("user_id")]
        public int UserId { get; private set; }

    }

    public class FriendRequest : Request
    {
        internal FriendRequest() { }
    }

    public class GroupRequest : FriendRequest // 为了减少反序列化次数，提高性能，继承关系去TMD（然而我也不知道反序列化要多久）。
    {
        internal const string AddType = "add";
        internal const string InvateType = "invite";

        internal GroupRequest() { }

        [JsonProperty("sub_type")]
        internal string SubType { get; private set; }

        [JsonProperty("group_id")]
        public int GroupId { get; private set; }
    }
}
