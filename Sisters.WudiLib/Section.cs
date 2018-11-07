using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 消息段。
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Section : IEquatable<Section>
    {
        public const string TextParamName = "text";
        public const string TextType = "text";
        public const string ImageType = "image";
        public const string RecordType = "record";
        public const string MusicType = "music";
        public const string AtType = "at";

        /// <summary>
        /// 仅支持大小写字母、数字、短横线（-）、下划线（_）及点号（.）。
        /// </summary>
        [JsonProperty("type")] private readonly string _type;

        [JsonProperty("data")]
        private readonly IReadOnlyDictionary<string, string> _data;

        public string Type => _type;
        [JsonIgnore]
        public IReadOnlyDictionary<string, string> Data => _data;

        [Obsolete("请改用 Data 属性", true)]
        public IReadOnlyDictionary<string, string> GetData() => Data;

        [JsonIgnore]
        internal string Raw
        {
            get
            {
                if (Type == TextType)
                    return Data[TextParamName].BeforeSend(false);
                var sb = new StringBuilder($"[CQ:{Type}");
                foreach (var param in Data)
                {
                    sb.Append($",{param.Key}={param.Value.BeforeSend()}");
                }

                sb.Append("]");
                return sb.ToString();
            }
        }

        public bool TryGetText(out string text)
        {
            bool isText = Type == TextType;
            text = isText ? Data.GetValueOrDefault(TextParamName, string.Empty) : default(string);
            return isText;
        }

        public bool TryGetAtMember(out long qq)
        {
            bool isAt = Type == AtType;
            qq = default(long);
            bool atMember = isAt && long.TryParse(
                s: Data.GetValueOrDefault("qq"),
                style: System.Globalization.NumberStyles.None,
                provider: System.Globalization.CultureInfo.InvariantCulture,
                result: out qq
            );
            return atMember;
        }

        internal Section(string type, params (string key, string value)[] p)
        {
            this._type = type;
            var data = new SortedDictionary<string, string>();
            Array.ForEach(p, pa => data.Add(pa.key, pa.value));
            this._data = new ReadOnlyDictionary<string, string>(data);
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <param name="jObject"></param>
        internal Section(Newtonsoft.Json.Linq.JToken jObject)
        {
            try
            {
                string type = jObject.Value<string>("type");
                _type = type;
                _data = jObject["data"].ToObject<ReadOnlyDictionary<string, string>>();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("构造消息段失败。\r\n" + jObject.ToString(), exception);
            }
        }

        public override bool Equals(object obj) => this.Equals(obj as Section);

        public bool Equals(Section other)
        {
            if (other is null)
                return false;
            if (this.Type != other.Type)
                return false;
            if (this.Data.Count != other.Data.Count)
                return false;
            foreach (var param in this.Data)
            {
                string key = param.Key;
                if (other.Data.TryGetValue(key, out string otherValue))
                    if (param.Value == otherValue)
                        continue;
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
                hashCode = hashCode * -1521134295 +
                           EqualityComparer<KeyValuePair<string, string>>.Default.GetHashCode(param);
            }

            //hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<string, string>>.Default.GetHashCode(this.data);
            return hashCode;
        }

        /// <summary>
        /// 将消息段转换为 <see cref="string"/>。如果类型为 <see cref="TextType"/>，则为文本内容（不转义）；否则与 CQ 码形式相同。
        /// </summary>
        /// <returns>转换后的 <see cref="string"/>。</returns>
        public override string ToString() => Type == TextType ? Data[TextParamName] : Raw;

        /// <summary>
        /// 构造文本消息段。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static Section Text(string text) => new Section(TextType, (TextParamName, text));

        /// <summary>
        /// 构造 At 消息段。
        /// </summary>
        /// <param name="qq"></param>
        /// <returns></returns>
        internal static Section At(long qq) => new Section(AtType, ("qq", qq.ToString()));

        /// <summary>
        /// 构造 At 全体成员消息段。
        /// </summary>
        /// <returns></returns>
        internal static Section AtAll() => new Section(AtType, ("qq", "all"));

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
            => noCache ? new Section(ImageType, ("cache", "0"), ("file", url)) : NetImage(url);

        internal static Section NetRecord(string url) => new Section(RecordType, ("file", url));

        internal static Section NetRecord(string url, bool noCache)
        {
            return noCache ? new Section(RecordType, ("cache", "0"), ("file", url)) : NetRecord(url);
        }

        /// <summary>
        /// 构造音乐自定义分享消息段。
        /// </summary>
        /// <param name="introductionUrl">分享链接，即点击分享后进入的音乐页面（如歌曲介绍页）。</param>
        /// <param name="audioUrl">音频链接（如mp3链接）。</param>
        /// <param name="title">音乐的标题，建议12字以内。</param>
        /// <param name="profile">音乐的简介，建议30字以内。该参数可被忽略。</param>
        /// <param name="imageUrl">音乐的封面图片链接。若参数为空或被忽略，则显示默认图片。</param>
        /// <exception cref="ArgumentException"><c>introductionUrl</c>或<c>audioUrl</c>或<c>title</c>为空。</exception>
        /// <exception cref="ArgumentNullException"><c>introductionUrl</c>或<c>audioUrl</c>或<c>title</c>为<c>null</c>。</exception>
        /// <returns></returns>
        internal static Section MusicCustom(string introductionUrl, string audioUrl, string title, string profile,
            string imageUrl)
        {
            const string introductionUrlParamName = "url";
            const string audioUrlParamName = "audio";
            const string titleParamName = "title";
            const string profileParamName = "content";
            const string imageUrlParamName = "image";
            Utilities.CheckStringArgument(introductionUrl, nameof(introductionUrl));
            Utilities.CheckStringArgument(audioUrl, nameof(audioUrl));
            Utilities.CheckStringArgument(title, nameof(title));
            var arguments = new List<(string argument, string value)>
                    {
                        (introductionUrlParamName, introductionUrl),
                        (audioUrlParamName, audioUrl),
                        (titleParamName, title),
                    };
            if (profile != null)
                arguments.Add((profileParamName, profile));
            if (!string.IsNullOrEmpty(imageUrl))
                arguments.Add((imageUrlParamName, imageUrl));
            return new Section(MusicType, arguments.ToArray());
        }

        internal static Section Shake() => new Section("shake");

        public static bool operator ==(Section left, Section right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(Section left, Section right) => !(left == right);
    }
}