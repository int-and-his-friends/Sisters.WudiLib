using System.Runtime.Serialization;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 表示禁言类型（禁言或解除禁言）。
    /// </summary>
    public enum GroupBanType
    {
        /// <summary>
        /// 禁言。
        /// </summary>
        [EnumMember(Value = "ban")]
        Ban,

        /// <summary>
        /// 解除禁言。
        /// </summary>
        [EnumMember(Value = "lift_ban")]
        LiftBan,
    }
}
