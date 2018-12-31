using System.Runtime.Serialization;

namespace Sisters.WudiLib.Responses
{
    /// <summary>
    /// 性别。
    /// </summary>
    public enum Sex
    {
        /// <summary>
        /// 未知。
        /// </summary>
        [EnumMember(Value = "unknown")]
        Unknown = 0,
        /// <summary>
        /// 男性。
        /// </summary>
        [EnumMember(Value = "male")]
        Male = 1,
        /// <summary>
        /// 女性。
        /// </summary>
        [EnumMember(Value = "female")]
        Female = 2,
    }
}
