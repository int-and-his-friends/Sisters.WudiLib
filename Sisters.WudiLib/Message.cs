using System.Collections.Generic;
using System.Linq;

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

        public abstract string Raw { get; }
    }

    public abstract partial class SectionMessage : Message
    {
        protected SectionMessage() => sections = new List<Section>();

        protected SectionMessage(IEnumerable<Section> sections)
            => this.sections = new List<Section>(sections);

        protected readonly IList<Section> sections;

        public override string Raw => GetRaw(sections);

        internal static string GetRaw(IEnumerable<Section> sections)
            => string.Join(string.Empty, sections.Select(section => section.Raw));
    }
}