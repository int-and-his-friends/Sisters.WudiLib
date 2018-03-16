using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 表示消息。
    /// </summary>
    public class Message
    {
        private readonly ICollection<Section> sections;

        internal ICollection<Section> Sections => sections;

        /// <summary>
        /// 指示此 <see cref="Message"/> 是否可以与其他 <see cref="Message"/> 连接。
        /// </summary>
        private readonly bool canJoin = true;

        /// <summary>
        /// 构造新的消息实例。
        /// </summary>
        public Message() => sections = new LinkedList<Section>();

        /// <summary>
        /// 从 <see cref="IEnumerable{Section}"/> 创建消息。
        /// </summary>
        /// <param name="sections"></param>
        internal Message(IEnumerable<Section> sections, bool canJoin = true)
        {
            this.sections = new LinkedList<Section>(sections);
            this.canJoin = canJoin;
        }

        /// <summary>
        /// 从文本构造新的消息实例。
        /// </summary>
        /// <param name="text">消息内容文本。</param>
        public Message(string text) : this() => sections.Add(Section.Text(text));

        /// <summary>
        /// 从两个 <see cref="Message"/> 实例创建消息。
        /// </summary>
        /// <param name="message1">在前面的消息。</param>
        /// <param name="message2">在后面的消息。</param>
        private Message(Message message1, Message message2) : this(message1.Sections.Union(message2.Sections))
        {
            if (!message1.canJoin || !message2.canJoin)
            {
                throw new InvalidOperationException("有一个或多个消息不能被连接。");
            }
        }

        /// <summary>
        /// 从 <see cref="Section"/> 实例创建消息。
        /// </summary>
        /// <param name="section">包含的消息段。</param>
        private Message(Section section, bool canJoin = true) : this()
        {
            sections.Add(section);
            this.canJoin = canJoin;
        }

        /// <summary>
        /// 构造 At 群、讨论组成员消息。
        /// </summary>
        /// <param name="qq">要 At 的 QQ 号。</param>
        /// <returns>构造的消息。</returns>
        public static Message At(long qq) => new Message(Section.At(qq));

        /// <summary>
        /// 构造 At 群、讨论组全体成员的消息。
        /// </summary>
        /// <returns>构造的消息。</returns>
        public static Message AtAll() => new Message(Section.AtAll());

        /// <summary>
        /// 构造包含本地图片的消息。
        /// </summary>
        /// <param name="file">本地图片的路径。</param>
        /// <returns>构造的消息。</returns>
        public static Message LocalImage(string file) => new Message(Section.LocalImage(file));

        /// <summary>
        /// 构造一条消息，包含来自网络的图片。
        /// </summary>
        /// <param name="url">网络图片 URL。</param>
        /// <returns>构造的消息。</returns>
        public static Message NetImage(string url) => new Message(Section.NetImage(url));

        /// <summary>
        /// 构造一条消息，包含来自网络的图片。可以指定是否不使用缓存。
        /// </summary>
        /// <param name="url">网络图片 URL。</param>
        /// <param name="noCache">是否不使用缓存（默认使用）。</param>
        /// <returns>构造的消息。</returns>
        public static Message NetImage(string url, bool noCache) => new Message(Section.NetImage(url, noCache));

        public static Message Shake() => new Message(Section.Shake(), false);

        /// <summary>
        /// 使用 <c>+</c> 连接两条消息。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Message operator +(Message left, Message right) => new Message(left, right);

        /// <summary>
        /// 消息段。
        /// </summary>
        internal class Section : IEquatable<Section>
        {
            /// <summary>
            /// 仅支持大小写字母、数字、短横线（-）、下划线（_）及点号（.）。
            /// </summary>
            [JsonProperty("type")]
            private readonly string type;

            [JsonIgnore]
            internal string Type => type;

            [JsonProperty("data")]
            private readonly IReadOnlyDictionary<string, string> data;

            [JsonIgnore]
            internal IReadOnlyDictionary<string, string> Data => data;

            [JsonIgnore]
            private string Raw
            {
                get
                {
                    if (Type == "text") return Data["text"].BeforeSend();
                    var sb = new StringBuilder($"[CQ:{Type}");
                    foreach (var param in Data)
                    {
                        sb.Append($",{param.Key}={param.Value.BeforeSend()}");
                    }
                    sb.Append("]");
                    return sb.ToString();
                }
            }

            private Section(string type, params (string key, string value)[] p)
            {
                this.type = type;
                var data = new SortedDictionary<string, string>();
                Array.ForEach(p, pa => data.Add(pa.key, pa.value));
                this.data = data;
            }

            /// <summary>
            /// 构造文本消息段。
            /// </summary>
            /// <param name="text"></param>
            /// <returns></returns>
            internal static Section Text(string text) => new Section("text", ("text", text));

            /// <summary>
            /// 构造 At 消息段。
            /// </summary>
            /// <param name="qq"></param>
            /// <returns></returns>
            internal static Section At(long qq) => new Section("at", ("qq", qq.ToString()));

            /// <summary>
            /// 构造 At 全体成员消息段。
            /// </summary>
            /// <returns></returns>
            internal static Section AtAll() => new Section("at", ("qq", "all"));

            /// <summary>
            /// 构造本地图片消息段。
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            internal static Section LocalImage(string file) => new Section("image", ("file", "file://" + file));

            /// <summary>
            /// 构造网络图片消息段。
            /// </summary>
            /// <param name="url"></param>
            /// <returns></returns>
            internal static Section NetImage(string url) => new Section("image", ("file", url));

            /// <summary>
            /// 构造网络图片消息段。可以指定是否使用缓存。
            /// </summary>
            /// <param name="url"></param>
            /// <param name="noCache">是否使用缓存。</param>
            /// <returns></returns>
            internal static Section NetImage(string url, bool noCache)
            {
                if (!noCache) return NetImage(url);
                return new Section("image", ("cache", "0"), ("file", url));
            }

            internal static Section Shake() => new Section("shake");

            public override bool Equals(object obj) => this.Equals(obj as Section);
            public bool Equals(Section other)
            {
                if (other == null) return false;
                if (this.Type != other.Type) return false;
                if (this.Data.Count != other.Data.Count) return false;
                foreach (var param in this.Data)
                {
                    string key = param.Key;
                    if (other.Data.TryGetValue(key, out string otherValue))
                        if (param.Value == otherValue) continue;
                    return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                var hashCode = -628614918;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.type);
                foreach (var param in Data)
                {
                    hashCode = hashCode * -1521134295 + EqualityComparer<KeyValuePair<string, string>>.Default.GetHashCode(param);
                }
                //hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<string, string>>.Default.GetHashCode(this.data);
                return hashCode;
            }

            public static bool operator ==(Section left, Section right)
            {
                if (left is null) return right is null;
                return left.Equals(right);
            }

            public static bool operator !=(Section left, Section right) => !(left == right);
        }
    }
}