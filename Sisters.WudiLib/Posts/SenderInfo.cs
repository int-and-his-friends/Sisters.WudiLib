using Newtonsoft.Json;
using Sisters.WudiLib.Responses;
using static Sisters.WudiLib.Responses.GroupMemberInfo;

namespace Sisters.WudiLib.Posts
{
    /// <summary>群消息发送人信息。</summary>
    public sealed class SenderInfo
    {
        /// <summary>发送者 QQ 号。</summary>
        [JsonProperty("user_id")] public long UserId { get; private set; }
        /// <summary>性别。可能会在以后改为枚举。</summary>
        [JsonProperty("sex")] public Sex Sex { get; private set; }

        /// <summary>昵称。</summary>
        [JsonProperty("nickname")] public string Nickname { get; private set; }
        /// <summary>年龄。</summary>
        [JsonProperty("age")] public int Age { get; private set; }

        /// <summary>群名片／备注。</summary>
        [JsonProperty("card")] public string InGroupName { get; private set; }
        /// <summary>地区。</summary>
        [JsonProperty("area")] public string Area { get; private set; }
        /// <summary>成员等级。</summary>
        [JsonProperty("level")] public string Level { get; private set; }

        /// <summary>角色。</summary>
        [JsonProperty("role"), JsonConverter(typeof(AuthorityConverter))]
        public GroupMemberAuthority Authority { get; private set; }

        /// <summary>专属头衔。</summary>
        [JsonProperty("title")] public string Title { get; private set; }
    }
}
