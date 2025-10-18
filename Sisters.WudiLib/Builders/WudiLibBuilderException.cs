using System;

namespace Sisters.WudiLib.Builders;

public class WudiLibBuilderException : Exception
{
    public WudiLibBuilderException() { }

    public WudiLibBuilderException(string message)
        : base(message) { }

    public WudiLibBuilderException(string message, Exception inner)
        : base(message, inner) { }
}
