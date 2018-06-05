using Newtonsoft.Json;

namespace Sisters.WudiLib.Posts
{
    public abstract class Event : Post
    {
        internal const string GroupAdminEvent = "group_admin";
        internal const string GroupDecreaseEvent = "group_decrease";
        internal const string GroupIncreaseEvent = "group_increase";

        [JsonProperty("event")]
        public string EventType { get; set; }
    }
}
