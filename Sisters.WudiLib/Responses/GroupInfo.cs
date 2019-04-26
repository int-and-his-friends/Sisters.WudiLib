using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Responses
{
    /// <summary>
    /// <c>get_group_list</c> 返回的群信息。
    /// </summary>
    public class GroupInfo
    {
        /// <summary>
        /// 群号。
        /// </summary>
        [JsonProperty("group_id")]
        public long Id { get; set; }

        /// <summary>
        /// 群名称。
        /// </summary>
        [JsonProperty("group_name")]
        public string Name { get; set; }
    }
}
