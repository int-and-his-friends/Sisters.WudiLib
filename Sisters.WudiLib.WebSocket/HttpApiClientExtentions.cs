using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket
{
    /// <summary>
    /// <see cref="HttpApiClient"/> 的扩展方法，主要用来响应请求。
    /// </summary>
    public static class HttpApiClientExtentions
    {
        /// <summary>
        /// 处理加好友请求。
        /// </summary>
        /// <param name="httpApiClient">HTTP API 客户端。</param>
        /// <param name="request">上报的加好友请求。</param>
        /// <param name="response">响应。</param>
        /// <exception cref="ApiAccessException">网络错误等。</exception>
        /// <exception cref="ArgumentNullException">某个参数为 <c>null</c>。</exception>
        /// <returns>是否成功</returns>
        public static Task<bool> HandleFriendRequestAsync(this HttpApiClient httpApiClient, FriendRequest request, FriendRequestResponse response)
        {
            if (httpApiClient == null)
            {
                throw new ArgumentNullException(nameof(httpApiClient));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var data = JObject.FromObject(response);
            data["flag"] = request.Flag;
            return httpApiClient.CallAsync("set_friend_add_request", data);
        }

        /// <summary>
        /// 处理加群请求／邀请。
        /// </summary>
        /// <param name="httpApiClient">HTTP API 客户端。</param>
        /// <param name="request">上报的加群邀请或请求。</param>
        /// <param name="response">响应。</param>
        /// <exception cref="ApiAccessException">网络错误等。</exception>
        /// <exception cref="ArgumentNullException">某个参数为 <c>null</c>。</exception>
        /// <returns>是否成功</returns>
        public static Task<bool> HandleGroupRequestAsync(this HttpApiClient httpApiClient, GroupRequest request, GroupRequestResponse response)
        {
            if (httpApiClient == null)
            {
                throw new ArgumentNullException(nameof(httpApiClient));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var data = JObject.FromObject(request);
            var responseData = JObject.FromObject(response);
            data.Merge(responseData);
            return httpApiClient.CallAsync("set_group_add_request", data);
        }
    }
}
