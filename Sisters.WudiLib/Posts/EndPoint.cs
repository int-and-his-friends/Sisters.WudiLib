using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 表示要将消息发送至的地点的类。
    /// </summary>
    public abstract class EndPoint
    {
        internal EndPoint() { }

        [JsonProperty("message_type")]
        internal string MessageType
        {
            get
            {
                const string suffix = nameof(EndPoint);
                //if (this is PrivateEndPoint) return Message.PrivateType;
                string type = this.GetType().Name;
                if (type.EndsWith(suffix))
                    type = type.Substring(0, type.Length - suffix.Length);
                return type.ToLowerInvariant();
            }
        }
    }

    public class PrivateEndPoint : EndPoint
    {
        internal PrivateEndPoint(long user) => this.UserId = user;

        [JsonProperty("user_id")]
        public long UserId { get; internal set; }
    }

    public class GroupEndPoint : EndPoint
    {
        internal GroupEndPoint(long group) => this.GroupId = group;

        [JsonProperty("group_id")]
        public long GroupId { get; private set; }
    }

    public class DiscussEndPoint : EndPoint
    {
        internal DiscussEndPoint(long discuss) => this.DiscussId = discuss;

        [JsonProperty("discuss_id")]
        public long DiscussId { get; private set; }
    }
}
