using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    public abstract class Response
    {
        [JsonProperty("block")]
        public bool Block { get; set; }
    }

    public class GroupRequestResponse : Response
    {
        [JsonProperty("approve")]
        public bool Approve { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    public class FriendRequestResponse : Response
    {
        [JsonProperty("approve")]
        public bool Approve { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }
    }

    public delegate GroupRequestResponse GroupRequestEventHandler(HttpApiClient api, GroupRequest request);

    public delegate FriendRequestResponse FriendRequestEventHandler(HttpApiClient api, FriendRequest request);

    public delegate bool PostResponser<in T>(T arg);
}
