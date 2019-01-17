using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 上报响应数据。
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Response
    {
        /// <summary>
        /// 是否拦截事件（不再让后面的插件处理）。
        /// </summary>
        [JsonProperty("block", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Block { get; set; }
    }

    /// <summary>
    /// 请求响应数据。
    /// </summary>
    public class RequestResponse : Response
    {
        /// 
        [JsonConstructor]
        public RequestResponse()
        {
        }

        /// <summary>
        /// 构造同意或者拒绝请求的响应。一般用于同意请求。
        /// </summary>
        /// <param name="approve">是否同意请求。</param>
        public RequestResponse(bool approve) => Approve = approve;
        /// <summary>
        /// 是否同意请求。
        /// </summary>
        [JsonProperty("approve", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Approve { get; set; }
    }

    /// 
    public sealed class GroupRequestResponse : RequestResponse
    {
        /// 
        [JsonConstructor]
        public GroupRequestResponse()
        {

        }

        /// <summary>
        /// 构造拒绝请求的响应。
        /// </summary>
        /// <param name="reason">拒绝理由。</param>
        public GroupRequestResponse(string reason)
        {
            Approve = false;
            Reason = reason;
        }

        /// <summary>
        /// 拒绝理由（仅在拒绝时有效）。
        /// </summary>
        [JsonProperty("reason", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [DefaultValue("")]
        public string Reason { get; set; }

        /// <summary>
        /// 从 <see cref="bool"/> 转换 <see cref="GroupRequestResponse"/>。
        /// </summary>
        /// <param name="approve"></param>
        public static implicit operator GroupRequestResponse(bool approve)
        {
            return new GroupRequestResponse
            {
                Approve = approve,
            };
        }
    }

    /// <summary>
    /// 加好友请求响应。
    /// </summary>
    public sealed class FriendRequestResponse : RequestResponse
    {
        /// <summary>
        /// 构造加好友响应。
        /// </summary>
        [JsonConstructor]
        public FriendRequestResponse()
        {
        }

        /// <summary>
        /// 构造接受请求响应。
        /// </summary>
        /// <param name="remark">好友备注。</param>
        public FriendRequestResponse(string remark)
        {
            Approve = true;
            Remark = remark;
        }

        /// <summary>
        /// 添加后的好友备注（仅在同意时有效）。
        /// </summary>
        [JsonProperty("remark", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [DefaultValue("")]
        public string Remark { get; set; }

        /// <summary>
        /// 从 <see cref="bool"/> 转换 <see cref="FriendRequestResponse"/>。
        /// </summary>
        /// <param name="approve"></param>
        public static implicit operator FriendRequestResponse(bool approve)
        {
            return new FriendRequestResponse
            {
                Approve = approve,
            };
        }
    }

    internal class MessageResponse : Response
    {
        public WudiLib.Message Reply { get; set; }

        [JsonProperty("reply")]
        private object _reply => Reply?.Serializing;
    }

    internal class MultiMessageResponse : MessageResponse
    {
        [JsonProperty("at_sender")]
        public bool AtSender { get; set; }
    }

    internal sealed class GroupMessageResponse : MultiMessageResponse
    {
        [JsonProperty("delete")]
        public bool Recall { get; set; }

        [JsonProperty("kick")]
        public bool Kick { get; set; }

        [JsonProperty("ban")]
        public bool Ban { get; set; }
    }
}
