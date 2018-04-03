using System;

namespace Sisters.WudiLib
{
    public class RawMessage : Message
    {
        private string _raw;

        public RawMessage(string raw) => _raw = raw ?? throw new ArgumentNullException(nameof(raw));

        public override string Raw => _raw;

        internal override object Serializing => _raw;

        public static RawMessage operator +(RawMessage left, RawMessage right)
            => new RawMessage(left._raw + right._raw);
    }
}
