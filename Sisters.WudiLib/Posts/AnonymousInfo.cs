using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 匿名用户信息。
    /// </summary>
    public sealed class AnonymousInfo
    {
        /// <summary>匿名用户 flag，在调用禁言 API 时需要传入。</summary>
        [JsonProperty("flag")]
        public string Flag { get; private set; }
        /// <summary>匿名用户 ID。</summary>
        [JsonProperty("id")]
        public int Id { get; private set; }
        /// <summary>匿名用户名称。</summary>
        [JsonProperty("name")]
        public string Name { get; private set; }
    }
}
