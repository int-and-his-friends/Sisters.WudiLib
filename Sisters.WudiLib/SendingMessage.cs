using System;
using System.Collections.Generic;
using System.Linq;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 表示将要发送的消息。
    /// </summary>
    public class SendingMessage : SectionMessage
    {
        private readonly static ICollection<string> NoJoinSectionTypes = new List<string>
        {
            "record",
            "rps",
            "dice",
            "music",
            "share",
        };

        internal IList<Section> Sections => sections;

        internal override object Serializing => sections;
        
        /// <summary>
        /// 指示此 <see cref="SendingMessage"/> 是否可以与其他 <see cref="SendingMessage"/> 连接。
        /// </summary>
        private bool CanConcat => Sections.Any(s => NoJoinSectionTypes.Contains(s.Type));

        /// <summary>
        /// 构造新的消息实例。
        /// </summary>
        public SendingMessage() : base() { }

        /// <summary>
        /// 从 <see cref="IEnumerable{Section}"/> 创建消息。
        /// </summary>
        /// <param name="sections"></param>
        internal SendingMessage(IEnumerable<Section> sections) : base(sections) { }

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
        /// <exception cref="InvalidOperationException">有无法连接的消息。</exception>
        private SendingMessage(SendingMessage message1, SendingMessage message2) : this(message1.Sections.Union(message2.Sections))
        {
            if (!message1.CanConcat || !message2.CanConcat)
            {
                throw new InvalidOperationException("有一个或多个消息不能被连接。");
            }
        }

        /// <summary>
        /// 从 <see cref="Section"/> 实例创建消息。
        /// </summary>
        /// <param name="section">包含的消息段。</param>
        private SendingMessage(Section section) : this()
        {
            sections.Add(section);
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

        public static SendingMessage NetRecord(string url, bool noCache) => new SendingMessage(Section.NetRecord(url, noCache));

        public static SendingMessage NetRecord(string url) => new SendingMessage(Section.NetRecord(url));

        public static SendingMessage Shake() => new SendingMessage(Section.Shake());

        /// <summary>
        /// 使用 <c>+</c> 连接两条消息。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <exception cref="InvalidOperationException">一个或多个消息不可连接。</exception>
        /// <returns></returns>
        public static SendingMessage operator +(SendingMessage left, SendingMessage right) => new SendingMessage(left, right);
    }
}