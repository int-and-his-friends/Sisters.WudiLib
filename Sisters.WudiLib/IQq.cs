using Sisters.WudiLib.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib
{
    public interface IQq
    {
        /// <summary>
        /// 在派生类中实现时，向目标QQ发送消息。
        /// </summary>
        /// <param name="userId">发送目标QQ号。</param>
        /// <param name="message">要发送的消息，将按照纯文本发送。</param>
        /// <returns>由 HTTP API 返回的数据。</returns>
        SendPrivateMessageResponseData SendPrivateMessage(long userId, string message);
    }
}
