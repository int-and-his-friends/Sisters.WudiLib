using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 各种消息类型的基类。
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// 返回发送时要序列化的对象。
        /// </summary>
        internal abstract object Serializing { get; }
    }

    public abstract class SectionMessage : Message
    {
        protected SectionMessage() => sections = new LinkedList<Section>();

        protected SectionMessage(IEnumerable<Section> sections)
            => this.sections = new LinkedList<Section>(sections);

        protected readonly ICollection<Section> sections;

        /// <summary>
        /// 消息段。
        /// </summary>
        protected internal class Section : IEquatable<Section>
        {
            public const string ImageType = "image";
            
            /// <summary>
            /// 仅支持大小写字母、数字、短横线（-）、下划线（_）及点号（.）。
            /// </summary>
            [JsonProperty("type")]
            private readonly string _type;

            [JsonIgnore]
            internal string Type => _type;

            [JsonProperty("data")]
            private readonly IReadOnlyDictionary<string, string> _data;

            [JsonIgnore]
            internal IReadOnlyDictionary<string, string> Data => _data;

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
                this._type = type;
                var data = new SortedDictionary<string, string>();
                Array.ForEach(p, pa => data.Add(pa.key, pa.value));
                this._data = data;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <exception cref="InvalidOperationException"></exception>
            /// <param name="jObject"></param>
            internal Section(Newtonsoft.Json.Linq.JToken jObject)
            {
                try
                {
                    string type = jObject.Value<string>("type");
                    _type = type;
                    var data = jObject["data"].ToObject<IReadOnlyDictionary<string, string>>();
                    _data = data;
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("构造消息段失败。\r\n" + jObject.ToString(), exception);
                }
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
            internal static Section LocalImage(string file) => new Section(ImageType, ("file", "file://" + file));

            /// <summary>
            /// 构造网络图片消息段。
            /// </summary>
            /// <param name="url"></param>
            /// <returns></returns>
            internal static Section NetImage(string url) => new Section(ImageType, ("file", url));

            /// <summary>
            /// 构造网络图片消息段。可以指定是否使用缓存。
            /// </summary>
            /// <param name="url"></param>
            /// <param name="noCache">是否使用缓存。</param>
            /// <returns></returns>
            internal static Section NetImage(string url, bool noCache)
            {
                if (!noCache) return NetImage(url);
                return new Section(ImageType, ("cache", "0"), ("file", url));
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
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this._type);
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

    /// <summary>
    /// 表示将要发送的消息。
    /// </summary>
    public class SendingMessage : SectionMessage
    {
        //private readonly ICollection<Section> sections;

        internal ICollection<Section> Sections => sections;

        internal override object Serializing => sections;

        /// <summary>
        /// 指示此 <see cref="SendingMessage"/> 是否可以与其他 <see cref="SendingMessage"/> 连接。
        /// </summary>
        private readonly bool canJoin = true;

        /// <summary>
        /// 构造新的消息实例。
        /// </summary>
        public SendingMessage() : base() { }

        /// <summary>
        /// 从 <see cref="IEnumerable{Section}"/> 创建消息。
        /// </summary>
        /// <param name="sections"></param>
        internal SendingMessage(IEnumerable<Section> sections, bool canJoin = true)
            : base(sections) => this.canJoin = canJoin;

        /// <summary>
        /// 从文本构造新的消息实例。
        /// </summary>
        /// <param name="text">消息内容文本。</param>
        public SendingMessage(string text) : this() => sections.Add(Section.Text(text));

        /// <summary>
        /// 从两个 <see cref="SendingMessage"/> 实例创建消息。
        /// </summary>
        /// <param name="message1">在前面的消息。</param>
        /// <param name="message2">在后面的消息。</param>
        private SendingMessage(SendingMessage message1, SendingMessage message2) : this(message1.Sections.Union(message2.Sections))
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
        private SendingMessage(Section section, bool canJoin = true) : this()
        {
            sections.Add(section);
            this.canJoin = canJoin;
        }

        /// <summary>
        /// 构造 At 群、讨论组成员消息。
        /// </summary>
        /// <param name="qq">要 At 的 QQ 号。</param>
        /// <returns>构造的消息。</returns>
        public static SendingMessage At(long qq) => new SendingMessage(Section.At(qq));

        /// <summary>
        /// 构造 At 群、讨论组全体成员的消息。
        /// </summary>
        /// <returns>构造的消息。</returns>
        public static SendingMessage AtAll() => new SendingMessage(Section.AtAll());

        /// <summary>
        /// 构造包含本地图片的消息。
        /// </summary>
        /// <param name="file">本地图片的路径。</param>
        /// <returns>构造的消息。</returns>
        public static SendingMessage LocalImage(string file) => new SendingMessage(Section.LocalImage(file));

        /// <summary>
        /// 构造一条消息，包含来自网络的图片。
        /// </summary>
        /// <param name="url">网络图片 URL。</param>
        /// <returns>构造的消息。</returns>
        public static SendingMessage NetImage(string url) => new SendingMessage(Section.NetImage(url));

        /// <summary>
        /// 构造一条消息，包含来自网络的图片。可以指定是否不使用缓存。
        /// </summary>
        /// <param name="url">网络图片 URL。</param>
        /// <param name="noCache">是否不使用缓存（默认使用）。</param>
        /// <returns>构造的消息。</returns>
        public static SendingMessage NetImage(string url, bool noCache) => new SendingMessage(Section.NetImage(url, noCache));

        public static SendingMessage Shake() => new SendingMessage(Section.Shake(), false);

        /// <summary>
        /// 使用 <c>+</c> 连接两条消息。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static SendingMessage operator +(SendingMessage left, SendingMessage right) => new SendingMessage(left, right);
    }
}