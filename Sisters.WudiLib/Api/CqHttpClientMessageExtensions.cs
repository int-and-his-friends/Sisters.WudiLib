using System.Threading.Tasks;
using Sisters.WudiLib.Api.Responses;

namespace Sisters.WudiLib.Api
{
    public static class CqHttpClientMessageExtensions
    {
        private const string PrivatePath = "send_private_msg";

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
            var result = await cqHttpClient.CallAsync<MessageResponseData>(PrivatePath, data);
            return result.IsOk ? result.Data : null;
        }
    }
}
