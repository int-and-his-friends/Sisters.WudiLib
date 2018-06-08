using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Sisters.WudiLib.Posts
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Post
    {
        internal const string MessagePost = "message";
        internal const string NoticePost = "event";
        internal const string RequestPost = "request";

        internal const string SubTypeField = "sub_type";

        internal Post() { }

        [JsonProperty("post_type")]
        internal string PostType { get; private set; }

        [JsonProperty("time")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset Time { get; private set; }
        [JsonProperty("self_id")]
        public long SelfId { get; private set; }
        [JsonProperty("user_id")]
        public long UserId { get; private set; }

        public abstract Endpoint Endpoint { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Request : Post
    {
        internal const string FriendType = "friend";
        internal const string GroupType = "group";

        internal Request() { }

        [JsonProperty("request_type")]
        internal string RequestType { get; private set; }

        [JsonProperty("flag")]
        public string Flag { get; private set; }
        [JsonProperty("message")]
        public string Message { get; private set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class FriendRequest : Request
    {
        internal FriendRequest() { }

        public override Endpoint Endpoint => new PrivateEndpoint(UserId);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GroupRequest : FriendRequest // 为了减少反序列化次数，提高性能，继承关系去TMD（然而我也不知道反序列化要多久）。
    {
        internal const string AddType = "add";
        internal const string InvateType = "invite";

        internal GroupRequest() { }

        [JsonProperty(SubTypeField)]
        internal string SubType { get; private set; }

        [JsonProperty("group_id")]
        public long GroupId { get; private set; }

        public override Endpoint Endpoint => new GroupEndpoint(GroupId);
    }
}
