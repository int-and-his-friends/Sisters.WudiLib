using System;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    public abstract class Response
    {
        [JsonProperty("block")]
        public bool Block { get; set; }
    }

    public class RequestResponse : Response
    {
        public RequestResponse()
        {
        }

        /// <summary>
        /// 构造拒绝请求的响应。
        /// </summary>
        /// <param name="reason">拒绝理由。</param>
        public RequestResponse(string reason)
        {
            Approve = false;
            Reason = reason;
        }

        /// <summary>
        /// 构造同意或者拒绝请求的响应。一般用于同意请求。
        /// </summary>
        /// <param name="approve">是否同意请求。</param>
        public RequestResponse(bool approve) => Approve = approve;

        [JsonProperty("approve")]
        public bool Approve { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    [Obsolete("RequestResponse")]
    public sealed class GroupRequestResponse : RequestResponse
    {
    }

    [Obsolete("RequestResponse")]
    public sealed class FriendRequestResponse : RequestResponse
    {
    }

    public class MessageResponse : Response
    {
        public WudiLib.Message Reply { get; set; }

        [JsonProperty("reply")]
        private object _reply => Reply?.Serializing;
    }

    public class MultiMessageResponse : MessageResponse
    {
        [JsonProperty("at_sender")]
        public bool AtSender { get; set; }
    }

    public sealed class GroupMessageResponse : MultiMessageResponse
    {
        [JsonProperty("delete")]
        public bool Recall { get; set; }

        [JsonProperty("kick")]
        public bool Kick { get; set; }

        [JsonProperty("ban")]
        public bool Ban { get; set; }
    }
}
