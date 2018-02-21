using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib.Responses
{
    internal class HttpApiResponse<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
        [JsonProperty("retcode")]
        public int Retcode { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    internal static class HttpApiResponse
    {
        public const int RetcodeOK = 0;
    }
}
