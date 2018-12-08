using System;

namespace Sisters.WudiLib
{
    [Serializable]
    public class CqHttpException : Exception
    {
        public CqHttpException() { }
        public CqHttpException(string message) : base(message) { }
        public CqHttpException(string message, Exception inner) : base(message, inner) { }
        protected CqHttpException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
