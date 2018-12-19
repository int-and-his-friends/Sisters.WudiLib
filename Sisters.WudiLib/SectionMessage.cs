using System;
using System.Collections.Generic;
using System.Linq;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 旧设计，正计划修改。
    /// </summary>
    public abstract class SectionMessage : Message
    {
        private protected SectionMessage() => SectionsBase = new List<Section>().AsReadOnly();

        /// <exception cref="ArgumentNullException"></exception>
        private protected SectionMessage(IEnumerable<Section> sections)
            => this.SectionsBase = new List<Section>(sections).AsReadOnly();

        /// <exception cref="ArgumentNullException"></exception>
        private protected SectionMessage(params Section[] sections) : this(sections as IEnumerable<Section>)
        { }

        protected virtual IReadOnlyList<Section> SectionsBase { get; }

        public override string Raw => GetRaw(SectionsBase);

        internal static string GetRaw(IEnumerable<Section> sections)
            => string.Join(string.Empty, sections.Select(section => section.Raw));
    }
}