using System;

namespace Sisters.WudiLib.Builders
{
    [Serializable]
    internal class WudiLibBuilderException : Exception
    {
        public WudiLibBuilderException() { }
        public WudiLibBuilderException(string message) : base(message) { }
        public WudiLibBuilderException(string message, Exception inner) : base(message, inner) { }
        protected WudiLibBuilderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
