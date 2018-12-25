using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sisters.WudiLib.Posts
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Post
    {
        internal const string Message = "message";
        internal const string Notice = "notice";
        internal const string Request = "request";

        internal const string TypeField = "post_type";
        internal const string SubTypeField = "sub_type";

        internal Post()
        {
            // ignored
        }

        [JsonProperty(TypeField)]
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
        internal const string Friend = "friend";
        internal const string Group = "group";

        internal new const string TypeField = "request_type";

        internal Request()
        {
            // ignored
        }

        [JsonProperty(TypeField)]
        internal string RequestType { get; private set; }
        [JsonProperty("flag")]
        public string Flag { get; private set; }
        [JsonProperty("comment")]
        public string Comment { get; private set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class FriendRequest : Request
    {
        internal FriendRequest()
        {
            // ignored
        }

        public override Endpoint Endpoint => new PrivateEndpoint(UserId);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GroupRequest : Request
    {
        internal const string Add = "add";
        internal const string Invite = "invite";

        internal GroupRequest()
        {
            // ignored
        }

        [JsonProperty(SubTypeField)]
        internal string SubType { get; private set; }

        [JsonProperty("group_id")]
        public long GroupId { get; private set; }

        public override Endpoint Endpoint => new GroupEndpoint(GroupId);
    }
}
