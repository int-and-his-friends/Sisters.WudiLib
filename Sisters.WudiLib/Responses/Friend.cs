using System.Runtime.Serialization;

namespace Sisters.WudiLib.Responses
{
    /// <summary>
    /// 好友。
    /// </summary>
    [DataContract]
    public sealed class Friend
    {
        /// <summary>
        /// QQ 号。
        /// </summary>
        [DataMember(Name = "user_id")]
        public long UserId { get; internal set; }

        /// <summary>
        /// 备注或昵称。
        /// </summary>
        [DataMember(Name = "remark")]
        public string Remark { get; internal set; }

        /// <summary>
        /// 昵称。
        /// </summary>
        [DataMember(Name = "nickname")]
        public string Nickname { get; internal set; }
    }
}
