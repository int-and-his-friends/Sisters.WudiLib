namespace Sisters.WudiLib.Posts
{
    public delegate GroupRequestResponse GroupRequestEventHandler(HttpApiClient api, GroupRequest request);

    public delegate FriendRequestResponse FriendRequestEventHandler(HttpApiClient api, FriendRequest request);

    public delegate void MessageEventHandler(HttpApiClient api, Message message);

    public delegate void AnonymousMessageEventHanlder(HttpApiClient api, AnonymousMessage message);

    public delegate void GroupNoticeEventHandler(HttpApiClient api, GroupMessage notice);
}
