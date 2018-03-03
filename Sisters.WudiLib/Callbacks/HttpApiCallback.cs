using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Sisters.WudiLib.Events;

namespace Sisters.WudiLib
{
    /// <summary>
    /// HttpApi的请求回调
    /// </summary>
    interface HttpApiCallback
    {
        /// <summary>
        /// 当接收请求时的回调
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <returns>是否处理请求</returns>
        bool OnRequest(HttpListenerContext context);

        /// <summary>
        /// 当收到上报事件时的回调
        /// </summary>
        /// <param name="e">事件</param>
        /// <returns>Http请求返回的响应</returns>
        string OnEvent(Event e);
    }
}
