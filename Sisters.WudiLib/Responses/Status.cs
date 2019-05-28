using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Responses
{
    /// <summary>
    /// 表示 CQ HTTP 插件运行状态。
    /// </summary>
    public class Status
    {
        /// <summary>
        /// HTTP API 插件已启用。
        /// </summary>
        [JsonProperty("app_enabled")]
        public bool AppEnabled { get; set; }

        /// <summary>
        /// HTTP API 插件正常运行（已初始化、已启用、各内部插件正常运行）。
        /// </summary>
        [JsonProperty("app_good")]
        public bool AppGood { get; set; }

        /// <summary>
        /// HTTP API 插件已初始化。
        /// </summary>
        [JsonProperty("app_initialized")]
        public bool AppInitialized { get; set; }

        /// <summary>
        /// HTTP API 插件状态符合预期，意味着插件已初始化，内部插件都在正常运行，且 QQ 在线。
        /// </summary>
        [JsonProperty("good")]
        public bool Good { get; set; }

        /// <summary>
        /// 当前 QQ 在线，<c>null</c> 表示无法查询到在线状态。
        /// </summary>
        [JsonProperty("online")]
        public bool? Online { get; set; }

        /// <summary>
        /// HTTP API 的各内部插件是否正常运行。
        /// </summary>
        [JsonProperty("plugins_good")]
        public IDictionary<string, bool> PluginsGood { get; set; }
    }
}
