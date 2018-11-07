namespace Sisters.WudiLib.Posts
{
    public delegate RequestResponse GroupRequestEventHandler(HttpApiClient api, GroupRequest request);

    public delegate RequestResponse FriendRequestEventHandler(HttpApiClient api, FriendRequest request);

    public delegate void MessageEventHandler(HttpApiClient api, Message message);

    public delegate void AnonymousMessageEventHanlder(HttpApiClient api, AnonymousMessage message);

    public delegate void GroupNoticeEventHandler(HttpApiClient api, GroupMessage notice);
}
