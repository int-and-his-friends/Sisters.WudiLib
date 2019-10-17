using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    public abstract class Notice : Post
    {
        internal const string GroupUpload = "group_upload";
        internal const string GroupAdmin = "group_admin";
        internal const string GroupDecrease = "group_decrease";
        internal const string GroupIncrease = "group_increase";
        internal const string GroupBan = "group_ban";
        internal const string FriendAdd = "friend_add";

        internal new const string TypeField = "notice_type";

        public abstract override Endpoint Endpoint { get; }
        [JsonProperty(TypeField)]
        internal string NoticeType { get; private set; }
    }

    public sealed class FriendAddNotice : Notice
    {
        public override Endpoint Endpoint => new PrivateEndpoint(UserId);
    }

    public abstract class GroupNotice : Notice
    {
        [JsonProperty("group_id")]
        public long GroupId { get; private set; }

        public override Endpoint Endpoint => new GroupEndpoint(GroupId);
    }

    /// <summary>
    /// 群组禁言事件。
    /// </summary>
    public sealed class GroupBanNotice : GroupNotice
    {
        /// <summary>
        /// 禁言类型（禁言或解除禁言）。
        /// </summary>
        [JsonProperty("sub_type")]
        public GroupBanType Type { get; private set; }

        [JsonProperty("operator_id")]
        public long OperatorId { get; private set; }

        /// <summary>
        /// 禁言时长（秒）。
        /// </summary>
        [JsonProperty("duration")]
        public int Duration { get; private set; }
    }

    public sealed class GroupFileNotice : GroupNotice
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
        public const string SetAdmin = "set";
        public const string UnsetAdmin = "unset";

        [JsonProperty(SubTypeField)]
        public string SubType { get; private set; }
    }

    public abstract class GroupMemberChangeNotice : GroupNotice
    {
        [JsonProperty(SubTypeField)]
        public string SubType { get; private set; }

        [JsonProperty("operator_id")]
        public long OperatorId { get; private set; }
    }

    public sealed class GroupMemberIncreaseNotice : GroupMemberChangeNotice
    {
        /// <summary>
        /// 表示管理员已同意入群。
        /// </summary>
        public const string AdminApprove = "approve";

        /// <summary>
        /// 表示管理员邀请入群。
        /// </summary>
        public const string AdminInvite = "invite";

        internal bool IsMe => UserId == SelfId;
    }

    public sealed class GroupMemberDecreaseNotice : GroupMemberChangeNotice
    {
        /// <summary>
        /// 表示主动退群。
        /// </summary>
        public const string Leave = "leave";

        /// <summary>
        /// 成员被踢。
        /// </summary>
        public const string Kick = "kick";
    }

    public sealed class KickedNotice : GroupMemberChangeNotice
    {
        internal const string Kicked = "kick_me";
    }
}
