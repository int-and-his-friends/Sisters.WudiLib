using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib.Responses
{
    /// <summary>
    /// 包含 QQ 号和昵称的登录信息
    /// </summary>
    public sealed class LoginInfo
    {
        private LoginInfo()
        {

        }

        [JsonProperty("user_id")]
        public long UserId { get; internal set; }

        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }
    }
}
