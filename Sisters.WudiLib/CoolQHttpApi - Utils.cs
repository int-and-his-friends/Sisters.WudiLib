using Newtonsoft.Json;
using Sisters.WudiLib.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sisters.WudiLib
{
    partial class CoolQHttpApi
    {
        /// <summary>
        /// 通过 POST 请求访问API
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="url">API请求地址</param>
        /// <param name="data">请求参数</param>
        /// <returns>从 HTTP API 返回的数据</returns>
        private async Task<T> PostAsync<T>(string url, object data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data), "data不能为null");
            string json = JsonConvert.SerializeObject(data);
            using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var http = new HttpClient())
            {
                using (var response = (await http.PostAsync(url, content)).EnsureSuccessStatusCode())
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<HttpApiResponse<T>>(responseContent);
                    return result.Retcode == HttpApiResponse<T>.RetcodeOK ? result.Data : default(T);
                }
            }
        }

        /// <summary>
        /// 通过 POST 请求访问API
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="url">API请求地址</param>
        /// <param name="data">请求参数</param>
        /// <returns>从 HTTP API 返回的数据</returns>
        private T Post<T>(string url, object data)
        {
            try
            {
                return PostAsync<T>(url, data).Result;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }
    }
}
