using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Api
{
    public class CqHttpClient : ICqHttpClient
    {
        private readonly string _accessToken = null;

        public CqHttpClient(string apiAddress)
        {
            ApiAddress = apiAddress.EndsWith("/", StringComparison.OrdinalIgnoreCase)
                ? apiAddress
                : apiAddress + "/";
        }

        public CqHttpClient(string apiAddress, string accessToken) : this(apiAddress) => _accessToken = accessToken;

        public string ApiAddress { get; }

        async Task<string> ICqHttpClient.CallAsync(string action, object args)
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
    }
}
