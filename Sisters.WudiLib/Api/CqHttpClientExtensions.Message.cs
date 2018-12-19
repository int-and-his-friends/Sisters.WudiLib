using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib.Api.Responses;

namespace Sisters.WudiLib.Api
{
    public partial class CqHttpClientExtensions
    {
        private const string PrivatePath = "send_private_msg";
        private const string GroupPath = "send_group_msg";
        private const string DiscussPath = "send_discuss_msg";
        private const string MessagePath = "send_msg";

        /// <summary>
        /// 发送私聊消息。
        /// </summary>
        /// <param name="userId">对方 QQ 号。</param>
        /// <param name="message">要发送的内容（文本）。</param>
        /// <returns>包含消息 ID 的响应数据。如果发送失败，则为 <c>null</c>。</returns>
        public static async Task<MessageResponseData> SendPrivateMessageAsync(this ICqHttpClient cqHttpClient, long userId, string message, bool throwsIfNotSucceed = false)
        {
            var data = new
            {
                user_id = userId,
                message,
                auto_escape = true,
            };
            return await cqHttpClient.GetResponse(PrivatePath, data, throwsIfNotSucceed);
        }

        ///
        public static async Task<MessageResponseData> SendGroupMessageAsync(this ICqHttpClient cqHttpClient, long groupId, string message, bool throwsIfNotSucceed = false)
        {
            var data = new
            {
                group_id = groupId,
                message,
                auto_escape = true,
            };
            return await cqHttpClient.GetResponse(PrivatePath, data, throwsIfNotSucceed);
        }

        ///
        public static async Task<MessageResponseData> SendDiscussMessageAsync(this ICqHttpClient cqHttpClient, long discussId, string message, bool throwsIfNotSucceed = false)
        {
            var data = new
            {
                discuss_id = discussId,
                message,
                auto_escape = true,
            };
            return await cqHttpClient.GetResponse(PrivatePath, data, throwsIfNotSucceed);
        }

        public static async Task<MessageResponseData> SendMessageAsync(this ICqHttpClient cqHttpClient, Posts.Endpoint endpoint, string message, bool throwsIfNotSucceed = false)
        {
            var data = JObject.FromObject(endpoint);
            data["message"] = JToken.FromObject(message);
            data["auto_escape"] = true;
            return await cqHttpClient.GetResponse(MessagePath, data, throwsIfNotSucceed);
        }

        private static async Task<MessageResponseData> GetResponse(this ICqHttpClient cqHttpClient, string path, object data, bool throws)
        {
            var result = await cqHttpClient.CallAsync<MessageResponseData>(path, data, throws);
            return result.IsOk ? result.Data : null;
        }
    }
}
