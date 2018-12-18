using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 表示将要发送的消息。
    /// </summary>
    public class SendingMessage : SectionMessage
    {
        private static readonly ICollection<string> NoJoinSectionTypes = new List<string>
        {
            "record",
            "rps",
            "dice",
            "music",
            "share",
        };

        public IReadOnlyList<Section> Sections => SectionsBase;

        internal override object Serializing => SectionsBase;

        /// <summary>
        /// 指示此 <see cref="SendingMessage"/> 是否可以与其他 <see cref="SendingMessage"/> 连接。
        /// </summary>
        private bool CanConcat => !Sections.Any(s => NoJoinSectionTypes.Contains(s.Type));

        /// <summary>
        /// 构造新的消息实例。
        /// </summary>
        public SendingMessage() : base()
        {
            // ignored
        }

        /// <summary>
        /// 从 <see cref="IEnumerable{Section}"/> 创建消息。
        /// </summary>
        /// <param name="sections"></param>
        internal SendingMessage(IEnumerable<Section> sections) : base(sections)
        {
            // ignored
        }

        /// <summary>
        /// 从文本构造新的消息实例。
        /// </summary>
        /// <param name="text">消息内容文本。</param>
        public SendingMessage(string text) : base(Section.Text(text))
        { }

        /// <summary>
        /// 从两个 <see cref="SendingMessage"/> 实例创建消息。
        /// </summary>
        /// <param name="message1">在前面的消息。</param>
        /// <param name="message2">在后面的消息。</param>
        /// <exception cref="InvalidOperationException">有无法连接的消息。</exception>
        private SendingMessage(SendingMessage message1, SendingMessage message2) : this(
            message1.Sections.Union(message2.Sections))
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
        public SendingMessage(Section section) : base(section)
        { }

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
        /// <param name="path">本地图片的路径。</param>
        /// <returns>构造的消息。</returns>
        public static SendingMessage LocalImage(string path) => new SendingMessage(Section.LocalImage(path));

        /// <summary>
        /// 构造包含本地图片的消息。可以把文件转换成 base64 形式，以便在其他机器上发送。
        /// </summary>
        /// <param name="path">本地图片的路径。</param>
        /// <param name="convertToBase64">是否要把图片消息转换为 base64 形式。</param>
        /// <exception cref="Exception">详见 <see cref="File.ReadAllBytes(string)"/> 所引发的异常。</exception>
        /// <returns>构造的消息。</returns>
        public static SendingMessage LocalImage(string path, bool convertToBase64 = false)
            => convertToBase64 ? ByteArrayImage(File.ReadAllBytes(path)) : LocalImage(path);

        /// <summary>
        /// 从 <see cref="byte"/> 数组构造消息。
        /// </summary>
        /// <param name="bytes">图片 <see cref="byte"/> 数组。</param>
        /// <exception cref="ArgumentNullException"><c>bytes</c> 为 <c>null</c>。</exception>
        /// <returns>构造的消息。</returns>
        public static SendingMessage ByteArrayImage(byte[] bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return new SendingMessage(Section.ByteArrayImage(bytes));
        }

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
        public static SendingMessage NetImage(string url, bool noCache) =>
            new SendingMessage(Section.NetImage(url, noCache));

        /// <summary>
        /// 网络语音。
        /// </summary>
        /// <param name="url">网络语音 URL。</param>
        /// <param name="noCache">是否不使用缓存（默认使用）。</param>
        /// <returns>网络语音消息。</returns>
        public static SendingMessage NetRecord(string url, bool noCache) =>
            new SendingMessage(Section.NetRecord(url, noCache));

        /// <summary>
        /// 网络语音。
        /// </summary>
        /// <param name="url">网络语音 URL。</param>
        /// <returns>网络语音消息。</returns>
        public static SendingMessage NetRecord(string url) => new SendingMessage(Section.NetRecord(url));

        /// <summary>
        /// 构造一条消息，包含音乐自定义分享，该分享指定了分享链接、音频链接、标题、简介和封面图片链接。
        /// </summary>
        /// <param name="introductionUrl">分享链接，即点击分享后进入的音乐页面（如歌曲介绍页）。</param>
        /// <param name="audioUrl">音频链接（如mp3链接）。</param>
        /// <param name="title">音乐的标题，建议12字以内。</param>
        /// <param name="profile">音乐的简介，建议30字以内。该参数可被忽略。</param>
        /// <param name="imageUrl">音乐的封面图片链接。若参数为空或被忽略，则显示默认图片。</param>
        /// <exception cref="ArgumentException"><c>introductionUrl</c>或<c>audioUrl</c>或<c>title</c>为空。</exception>
        /// <exception cref="ArgumentNullException"><c>introductionUrl</c>或<c>audioUrl</c>或<c>title</c>为<c>null</c>。</exception>
        /// <returns>包含该音乐自定义分享的消息。</returns>
        public static SendingMessage MusicCustom(string introductionUrl, string audioUrl, string title, string profile,
            string imageUrl)
            => new SendingMessage(Section.MusicCustom(introductionUrl, audioUrl, title, profile, imageUrl));

        /// <summary>
        /// 构造一条消息，包含音乐自定义分享，该分享指定了分享链接、音频链接和标题。
        /// </summary>
        /// <param name="introductionUrl">分享链接，即点击分享后进入的音乐页面（如歌曲介绍页）。</param>
        /// <param name="audioUrl">音频链接（如mp3链接）。</param>
        /// <param name="title">音乐的标题，建议12字以内。</param>
        /// <exception cref="ArgumentException"><c>introductionUrl</c>或<c>audioUrl</c>或<c>title</c>为空。</exception>
        /// <exception cref="ArgumentNullException"><c>introductionUrl</c>或<c>audioUrl</c>或<c>title</c>为<c>null</c>。</exception>
        /// <returns>包含该音乐自定义分享的消息。</returns>
        public static SendingMessage MusicCustom(string introductionUrl, string audioUrl, string title)
            => new SendingMessage(Section.MusicCustom(introductionUrl, audioUrl, title, null, null));

        /// <summary>
        /// 戳一戳。
        /// </summary>
        public static SendingMessage Shake() => new SendingMessage(Section.Shake());

        /// <summary>
        /// 使用 <c>+</c> 连接两条消息。
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <exception cref="InvalidOperationException">一个或多个消息不可连接。</exception>
        /// <returns></returns>
        public static SendingMessage operator +(SendingMessage left, SendingMessage right) =>
            new SendingMessage(left, right);
    }
}