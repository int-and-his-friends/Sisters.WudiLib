using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sisters.WudiLib
{
#if NET6_0_OR_GREATER
#nullable enable
    [InterpolatedStringHandler]
    public ref struct MessageInterpolatedStringHandler
    {
        private readonly List<Section> _builder;

        public MessageInterpolatedStringHandler(int literalLength, int formattedCount)
        {
            _builder = new List<Section>(literalLength);
        }

        public void AppendLiteral(string s)
        {
            _builder.Add(Section.Text(s));
        }

        public void AppendFormatted<T>(T t)
        {
            if (t is Section section)
            {
                _builder.Add(section);
            }
            else if (t is SendingMessage sendingMessage)
            {
                _builder.AddRange(sendingMessage.Sections);
            }
            else if (t is RawMessage)
            {
                throw new ArgumentException("用内联字符串构建消息实例时不支持包含 RawMessage。", nameof(t));
            }
            else
            {
                _builder.Add(Section.Text(t?.ToString()));
            }
        }

        public void AppendFormatted<T>(T t, string format) where T : IFormattable
        {
            _builder.Add(Section.Text(t?.ToString(format, null)));
        }

        internal SendingMessage GetFormattedMessage() => new(_builder);
    }
#nullable restore
#endif
}
