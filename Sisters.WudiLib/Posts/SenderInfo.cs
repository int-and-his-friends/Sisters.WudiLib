using Newtonsoft.Json;
using static Sisters.WudiLib.Responses.GroupMemberInfo;

namespace Sisters.WudiLib.Posts
{
    public class SenderInfo
    {
        [JsonProperty("user_id")] public long UserId { get; private set; }
        [JsonProperty("sex")] public string Sex { get; private set; }
        [JsonProperty("nickname")] public string Nickname { get; private set; }
        [JsonProperty("age")] public int Age { get; private set; }

        [JsonProperty("card")] public string InGroupName { get; private set; }
        [JsonProperty("area")] public string Area { get; private set; }
        [JsonProperty("level")] public string Level { get; private set; }

        [JsonProperty("role"), JsonConverter(typeof(AuthorityConverter))]
        public GroupMemberAuthority Authority { get; private set; }

        [JsonProperty("title")] public string Title { get; private set; }
    }
}
