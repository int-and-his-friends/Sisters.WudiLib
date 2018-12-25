using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.Posts
{
    partial class ApiPostListener
    {
        //事件上报？
        internal static Post GetPost(string json)
        {
            // TODO
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
                    result = GetNoticePost(contentObject);
                    break;
                case Post.Request:
                    result = GetRequestPost(contentObject);
                    break;
            }

            return result;
        }

        private static Message GetMessagePost(JObject jObject)
        {
            Message result = null;
            switch (jObject[Message.TypeField].ToString())
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
                default:
                    throw new Exception("消息事件TypeField错误");
            }

            return result;
        }

        private static Notice GetNoticePost(JObject jObject)
        {
            Notice result = null;
            switch (jObject[Notice.TypeField].ToString())
            {
                case Notice.FriendAdd:
                    result = jObject.ToObject<FriendAddNotice>();
                    break;
                case Notice.GroupAdmin:
                    result = jObject.ToObject<GroupAdminNotice>();
                    break;
                case Notice.GroupDecrease:
                    result = jObject.ToObject<GroupMemberDecreaseNotice>();
                    break;
                case Notice.GroupIncrease:
                    result = jObject.ToObject<GroupMemberIncreaseNotice>();
                    break;
                case Notice.GroupUpload:
                    result = jObject.ToObject<GroupFileNotice>();
                    break;
                case Notice.Request:
                //TODO:?what?
                default:
                    throw new Exception("通知事件TypeField错误");
            }

            return result;
        }

        private static Request GetRequestPost(JObject jObject)
        {
            Request result;
            switch (jObject[Request.TypeField].ToString())
            {
                case Request.Friend:
                    result = jObject.ToObject<FriendRequest>();
                    break;
                case Request.Group:
                    result = jObject.ToObject<GroupRequest>();
                    break;
                default:
                    throw new Exception("请求事件TypeField错误");
            }

            return result;
        }
    }
}