using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Api
{
    /// <summary>
    /// 包含泛型的响应。
    /// </summary>
    /// <typeparam name="T">响应数据类型。</typeparam>
    public class CqHttpApiResponse<T> : CqHttpApiResponse
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }

    /// <summary>
    /// 响应。
    /// </summary>
    public class CqHttpApiResponse
    {
        public const int RetcodeOK = 0;
        public static System.Collections.ObjectModel.ReadOnlyCollection<int> AcceptableRetcodes { get; } = new List<int>
        {
            0,
            1,
        }.AsReadOnly();

        /// <summary>
        /// 如果 <see cref="Retcode"/> 是 <c>0</c>，则为 <c>true</c>；否则为 <c>false</c>。
        /// </summary>
        public bool IsOk => RetcodeOK == Retcode;
        /// <summary>
        /// 如果 <see cref="Retcode"/> 是 <c>0</c> 或 <c>1</c>，则为 <c>true</c>；否则为 <c>false</c>。
        /// </summary>
        public bool IsAcceptableStatus => AcceptableRetcodes.Contains(Retcode);

        [JsonProperty("retcode")]
        public int Retcode { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
