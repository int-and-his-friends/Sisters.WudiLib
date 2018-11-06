using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 表示要将消息发送至的地点的类。
    /// </summary>
    public abstract class Endpoint : IEquatable<Endpoint>//, IComparable<Endpoint>
    {
        internal Endpoint()
        {
            // ignored
        }

        [JsonProperty("message_type")]
        internal string MessageType
        {
            get
            {
                const string suffix = nameof(Endpoint);
                string type = this.GetType().Name;
                if (type.EndsWith(suffix, StringComparison.Ordinal))
                    type = type.Substring(0, type.Length - suffix.Length);
                return type.ToLowerInvariant();
            }
        }

        internal static Endpoint FromMessage(Message message)
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

        public abstract override bool Equals(object obj);
        public bool Equals(Endpoint other) => this.Equals(other as object);

        public abstract override int GetHashCode();

        public static bool operator ==(Endpoint endpoint1, Endpoint endpoint2) => EqualityComparer<Endpoint>.Default.Equals(endpoint1, endpoint2);
        public static bool operator !=(Endpoint endpoint1, Endpoint endpoint2) => !(endpoint1 == endpoint2);
    }

    public sealed class PrivateEndpoint : Endpoint, IEquatable<PrivateEndpoint>
    {
        public PrivateEndpoint(long user) => this.UserId = user;

        [JsonProperty("user_id")]
        public long UserId { get; private set; }

        public override bool Equals(object obj) => Equals(obj as PrivateEndpoint);
        public bool Equals(PrivateEndpoint other) => other != null && UserId == other.UserId;

        public override int GetHashCode()
        {
            var hashCode = 1708038101;
            hashCode = hashCode * -1521134295 + UserId.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(PrivateEndpoint endpoint1, PrivateEndpoint endpoint2) => EqualityComparer<PrivateEndpoint>.Default.Equals(endpoint1, endpoint2);
        public static bool operator !=(PrivateEndpoint endpoint1, PrivateEndpoint endpoint2) => !(endpoint1 == endpoint2);
    }

    public sealed class GroupEndpoint : Endpoint, IEquatable<GroupEndpoint>
    {
        public GroupEndpoint(long group) => this.GroupId = group;

        [JsonProperty("group_id")]
        public long GroupId { get; private set; }

        public override bool Equals(object obj) => Equals(obj as GroupEndpoint);
        public bool Equals(GroupEndpoint other) => other != null && GroupId == other.GroupId;

        public override int GetHashCode()
        {
            var hashCode = -1449488233;
            hashCode = hashCode * -1521134295 + GroupId.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(GroupEndpoint endpoint1, GroupEndpoint endpoint2) => EqualityComparer<GroupEndpoint>.Default.Equals(endpoint1, endpoint2);
        public static bool operator !=(GroupEndpoint endpoint1, GroupEndpoint endpoint2) => !(endpoint1 == endpoint2);
    }

    public sealed class DiscussEndpoint : Endpoint, IEquatable<DiscussEndpoint>
    {
        public DiscussEndpoint(long discuss) => this.DiscussId = discuss;

        [JsonProperty("discuss_id")]
        public long DiscussId { get; private set; }

        public override bool Equals(object obj) => Equals(obj as DiscussEndpoint);
        public bool Equals(DiscussEndpoint other) => other != null && DiscussId == other.DiscussId;

        public override int GetHashCode()
        {
            var hashCode = -54904678;
            hashCode = hashCode * -1521134295 + DiscussId.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(DiscussEndpoint endpoint1, DiscussEndpoint endpoint2) => EqualityComparer<DiscussEndpoint>.Default.Equals(endpoint1, endpoint2);
        public static bool operator !=(DiscussEndpoint endpoint1, DiscussEndpoint endpoint2) => !(endpoint1 == endpoint2);
    }
}
