using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Api
{
    /// <summary>
    /// API客户端。
    /// </summary>
    public class CqHttpClient : ICqHttpClient
    {
        private readonly string _accessToken = null;

        /// <param name="apiAddress">插件的监听地址。用来访问 API。</param>
        /// <exception cref="ArgumentNullException"><c>apiAddress</c> 为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException"><c>apiAddress</c> 只包含空白字符。</exception>
        public CqHttpClient(string apiAddress)
        {
            if (string.IsNullOrWhiteSpace(apiAddress))
            {
                throw apiAddress is null
                    ? new ArgumentNullException(nameof(apiAddress))
                    : new ArgumentException("message", nameof(apiAddress));
            }

            ApiAddress = apiAddress.EndsWith("/", StringComparison.OrdinalIgnoreCase)
                ? apiAddress
                : apiAddress + "/";
        }

        /// <param name="apiAddress">插件的监听地址。用来访问 API。</param>
        /// <param name="accessToken">看变量名就该明白了吧。</param>
        /// <exception cref="ArgumentNullException"><c>apiAddress</c> 为 <c>null</c>。</exception>
        /// <exception cref="ArgumentException"><c>apiAddress</c> 只包含空白字符。</exception>
        public CqHttpClient(string apiAddress, string accessToken) : this(apiAddress) => _accessToken = accessToken;

        public string ApiAddress { get; }

        protected virtual async Task<string> CallAsync(string action, object args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var url = ApiAddress + action;

            string json = JsonConvert.SerializeObject(args);
            using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var http = new HttpClient())
            {
                if (!string.IsNullOrEmpty(_accessToken))
                {
                    http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Token " + _accessToken);
                }
                using (var response = (await http.PostAsync(url, content)).EnsureSuccessStatusCode())
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        Task<string> ICqHttpClient.CallAsync(string action, object args) => CallAsync(action, args);
    }
}
