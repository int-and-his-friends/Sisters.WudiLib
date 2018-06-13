using System.Collections.Generic;
using System.Linq;

namespace Sisters.WudiLib
{
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