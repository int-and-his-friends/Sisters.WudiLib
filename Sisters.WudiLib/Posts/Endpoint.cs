using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 表示要将消息发送至的地点的类。
    /// </summary>
    public abstract class Endpoint
    {
        internal Endpoint() { }

        [JsonProperty("message_type")]
        internal string MessageType
        {
            get
            {
                const string suffix = nameof(Endpoint);
                //if (this is PrivateEndPoint) return Message.PrivateType;
                string type = this.GetType().Name;
                if (type.EndsWith(suffix))
                    type = type.Substring(0, type.Length - suffix.Length);
                return type.ToLowerInvariant();
            }
        }

        internal Endpoint FromMessage(Message message)
        {
            switch (message)
            {
                case PrivateMessage p:
                    return new PrivateEndpoint(p.UserId);
                case GroupMessage g:
                    return new GroupEndpoint(g.GroupId);
                case DiscussMessage d:
                    return new DiscussEndpoint(d.DiscussId);
                default:
                    break;
            }
            return null;
        }
    }

    public sealed class PrivateEndpoint : Endpoint
    {
        internal PrivateEndpoint(long user) => this.UserId = user;

        [JsonProperty("user_id")]
        public long UserId { get; internal set; }
    }

    public sealed class GroupEndpoint : Endpoint
    {
        internal GroupEndpoint(long group) => this.GroupId = group;

        [JsonProperty("group_id")]
        public long GroupId { get; private set; }
    }

    public sealed class DiscussEndpoint : Endpoint
    {
        internal DiscussEndpoint(long discuss) => this.DiscussId = discuss;

        [JsonProperty("discuss_id")]
        public long DiscussId { get; private set; }
    }
}
