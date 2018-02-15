using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Sisters.WudiLib.Responses;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 通过酷Q HTTP API实现QQ功能。
    /// </summary>
    public partial class CoolQHttpApi : IQq
    {
        private string apiAddress;

        /// <summary>
        /// 获取或设置 HTTP API 的监听地址
        /// </summary>
        public string ApiAddress
        {
            get => apiAddress;
            set => apiAddress = value.TrimEnd('/');
        }

        public SendPrivateMessageResponseData SendPrivateMessage(long userId, string message)
        {
            var data = new
            {
                user_id = userId,
                message,
                auto_escape = true,
            };
            var result = Post<SendPrivateMessageResponseData>(PrivateUrl, data);
            return result;
        }
    }
}
