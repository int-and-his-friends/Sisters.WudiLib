using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    public abstract class Notice : Post
    {
        internal const string GroupUploadNotice = "group_upload";
        internal const string GroupAdminNotice = "group_admin";
        internal const string GroupDecreaseNotice = "group_decrease";
        internal const string GroupIncreaseNotice = "group_increase";
        internal const string FriendAddNotice = "friend_add";
        internal const string NoticeField = "notice_type";

        public abstract override Endpoint Endpoint { get; }
        [JsonProperty(NoticeField)]
        internal string NoticeType { get; private set; }
    }

    public sealed class FriendAddNotice : Notice
    {
        public override Endpoint Endpoint => new PrivateEndpoint(UserId);
    }

    public abstract class GroupNotice : Notice
    {
        public long GroupId { get; private set; }

        public override Endpoint Endpoint => new GroupEndpoint(GroupId);
    }

    public sealed class GroupUploadNotice : GroupNotice
    {
        [JsonProperty("file")]
        public GroupFile File { get; private set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class GroupFile
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("size")]
        public long Length { get; private set; }

        [JsonProperty("busid")]
        public int BusId { get; private set; }
    }

    public sealed class GroupAdminNotice : GroupNotice
    {
        internal const string SetAdmin = "set";
        internal const string UnsetAdmin = "unset";

        [JsonProperty(SubTypeField)]
        internal string SubType { get; private set; }
    }

    public sealed class GroupMemberChangeNotice : GroupNotice
    {
        [JsonProperty(SubTypeField)]
        internal string SubType { get; private set; }

        [JsonProperty("operator_id")]
        internal long OperatorId { get; private set; }
    }
}
