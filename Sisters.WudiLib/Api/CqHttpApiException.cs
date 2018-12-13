using System;
using Sisters.WudiLib.Api.Responses;

namespace Sisters.WudiLib.Api
{
    [Serializable]
    public class CqHttpApiException : CqHttpException
    {
        public CqHttpApiException() { }
        public CqHttpApiException(string message) : base(message) { }
        public CqHttpApiException(string message, Exception inner) : base(message, inner) { }
        public CqHttpApiException(string message, CqHttpApiResponse response) : base(message) => Response = response;
        protected CqHttpApiException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// 如果是因为执行失败而抛出的异常，则此属性表示返回的响应。
        /// </summary>
        public CqHttpApiResponse Response { get; }
    }
}
