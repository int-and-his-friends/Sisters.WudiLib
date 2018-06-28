using System.Collections.Generic;
using System.Linq;

namespace Sisters.WudiLib
{
    public abstract partial class SectionMessage : Message
    {
        protected SectionMessage() => SectionsBase = new List<Section>();

        protected SectionMessage(IEnumerable<Section> sections)
            => this.SectionsBase = new List<Section>(sections);

        protected readonly IList<Section> SectionsBase;

        public override string Raw => GetRaw(SectionsBase);

        internal static string GetRaw(IEnumerable<Section> sections)
            => string.Join(string.Empty, sections.Select(section => section.Raw));
    }
}