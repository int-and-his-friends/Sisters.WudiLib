using System.Threading.Tasks;

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
        /// <returns>包含消息 ID 的响应数据。</returns>
        public static async Task SendPrivateMessageAsync(this ICqHttpClient cqHttpClient, long userId, string message)
        {
            var data = new
            {
                user_id = userId,
                message,
                auto_escape = true,
            };
            var result = await cqHttpClient.CallNoDataAsync(PrivatePath, data);
            //return result;
        }
    }
}
