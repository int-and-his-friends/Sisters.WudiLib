using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Api
{
    /// <summary>
    /// <see cref="ICqHttpClient"/> 的扩展方法。
    /// </summary>
    public static class CqHttpClientExtensions
    {
        /// <summary>
        /// 调用指定 API，并指定返回数据。可以在调用失败时引发异常。
        /// </summary>
        /// <typeparam name="T">返回数据类型。</typeparam>
        /// <param name="cqHttpClient">API 客户端。</param>
        /// <param name="action">要调用的 API 功能。</param>
        /// <param name="args">参数。</param>
        /// <param name="throwsIfNotSucceed">是否在 <see cref="CqHttpApiResponse.Retcode"/> 显示未成功时引发异常。</param>
        /// <returns>响应。</returns>
        /// <exception cref="ArgumentNullException"><c>cqHttpClient</c> 为 <c>null</c>。</exception>
        /// <exception cref="CqHttpApiException">调用过程出现异常。</exception>
        public static async Task<CqHttpApiResponse<T>> CallAsync<T>(this ICqHttpClient cqHttpClient, string action, object args, bool throwsIfNotSucceed = false)
        {
            return await cqHttpClient.CallPrivateAsync<CqHttpApiResponse<T>>(action, args, throwsIfNotSucceed);
        }

        internal static async Task<CqHttpApiResponse> CallNoDataAsync(this ICqHttpClient cqHttpClient, string action, object args, bool throwsIfNotSucceed = false)
        {
            return await cqHttpClient.CallPrivateAsync<CqHttpApiResponse>(action, args, throwsIfNotSucceed);
        }

        private static async Task<T> CallPrivateAsync<T>(this ICqHttpClient cqHttpClient, string action, object args, bool throwsIfNotSucceed) where T : CqHttpApiResponse
        {
            if (cqHttpClient == null)
            {
                throw new ArgumentNullException(nameof(cqHttpClient));
            }

            try
            {
                string response = await cqHttpClient.CallAsync(action ?? string.Empty, args ?? new object());
                var result = JsonConvert.DeserializeObject<T>(response);
                if (throwsIfNotSucceed && !result.IsAcceptableStatus)
                {
                    throw new CqHttpApiException(result.Status);
                }
                return result;
            }
            catch (Exception e) when (!(e is CqHttpApiException))
            {
                throw new CqHttpApiException(e.Message, e);
            }
        }
    }
}
