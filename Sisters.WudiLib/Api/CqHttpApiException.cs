using System;

namespace Sisters.WudiLib.Api
{
    [Serializable]
    public class CqHttpApiException : CqHttpException
    {
        public CqHttpApiException() { }
        public CqHttpApiException(string message) : base(message) { }
        public CqHttpApiException(string message, Exception inner) : base(message, inner) { }
        protected CqHttpApiException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
