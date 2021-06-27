using System;

namespace Sisters.WudiLib.Builders
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Serializable]
    public class WudiLibBuilderException : Exception
    {
        public WudiLibBuilderException() { }
        public WudiLibBuilderException(string message) : base(message) { }
        public WudiLibBuilderException(string message, Exception inner) : base(message, inner) { }
        protected WudiLibBuilderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
