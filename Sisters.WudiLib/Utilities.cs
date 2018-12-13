using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sisters.WudiLib.Api.Responses;

namespace Sisters.WudiLib
{
    internal static class Utilities
    {
        private static async Task<CqHttpApiResponse<T>> PostApiAsync<T>(string url, object data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data), "data不能为null");
            try
            {
                string json = JsonConvert.SerializeObject(data);
                using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var http = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(HttpApiClient.AccessToken))
                    {
                        //content.Headers.Add("Authorization", "Token " + HttpApiClient.AccessToken);
                        http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Token " + HttpApiClient.AccessToken);
                    }
                    using (var response = (await http.PostAsync(url, content)).EnsureSuccessStatusCode())
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<CqHttpApiResponse<T>>(responseContent);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                throw new ApiAccessException("访问 API 时出现错误。", e);
            }
        }

        /// <summary>
        /// 通过 POST 请求访问API，返回数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="url">API请求地址</param>
        /// <param name="data">请求参数</param>
        /// <returns>从 HTTP API 返回的数据</returns>
        public static async Task<T> PostAsync<T>(string url, object data)
        {
            var response = await PostApiAsync<T>(url, data);
            return response.Retcode == CqHttpApiResponse.RetcodeOK ? response.Data : default(T);
        }
        
        /// <exception cref="ApiAccessException">网络错误等。</exception>
        public static async Task<bool> PostAsync(string url, object data)
        {
            try
            {
                var response = await PostApiAsync<object>(url, data);
                return response.Retcode == CqHttpApiResponse.RetcodeOK;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
        
        /// <summary>
        /// 检查<see cref="string"/>是否为<c>null</c>或空值，并抛出相应的异常。
        /// </summary>
        /// <param name="argument">要检查的<see cref="string"/>。</param>
        /// <param name="paramName">TODO</param>
        /// <exception cref="ArgumentException"><c>argument</c>为空。</exception>
        /// <exception cref="ArgumentNullException"><c>argument</c>为<c>null</c>。</exception>
        internal static void CheckStringArgument(string argument, string paramName)
        {
            if (argument is null)
                throw new ArgumentNullException(paramName);
            if (argument.Length == 0)
                throw new ArgumentException($"{paramName}为空。", paramName);
        }
    }
}
