using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    public delegate GroupRequestResponse GroupRequestEventHandler(HttpApiClient api, GroupRequest request);

    public delegate FriendRequestResponse FriendRequestEventHandler(HttpApiClient api, FriendRequest request);

    public delegate void MessageEventHandler(HttpApiClient api, Message message);

    public delegate bool PostResponser<in T>(T arg) where T : Response;
}
