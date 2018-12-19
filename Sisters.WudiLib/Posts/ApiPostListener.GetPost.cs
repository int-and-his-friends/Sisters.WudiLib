using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.Posts
{
    partial class ApiPostListener
    {
        internal static Post GetPost(string json)
        {// TODO
            if (string.IsNullOrEmpty(json))
                return null;

            JObject contentObject = JsonConvert.DeserializeObject<JObject>(json);
            Post result = null;
            switch (contentObject[Post.TypeField].ToObject<string>())
            {
                case Post.Message:
                    result = GetMessagePost(contentObject);
                    break;
                case Post.Notice:
                    // TODO
                    break;
                case Post.Request:
                    // TODO
                    break;
            }

            return null;
        }

        private static Message GetMessagePost(JObject jObject)
        {
            Message result = null;
            switch (jObject[Message.TypeField].ToObject<string>())
            {
                case Message.PrivateType:
                    result = jObject.ToObject<PrivateMessage>();
                    break;
                case Message.GroupType:
                    result = jObject.ToObject<GroupMessage>();
                    break;
                case Message.DiscussType:
                    result = jObject.ToObject<DiscussMessage>();
                    break;
            }
            return result;
        }
    }
}
