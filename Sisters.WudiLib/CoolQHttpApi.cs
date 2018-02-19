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

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        /// <param name="userId">对方 QQ 号</param>
        /// <param name="message">要发送的内容（文本）</param>
        /// <returns></returns>
        public SendPrivateMessageResponseData SendPrivateMessage(long userId, string message)
        {
            var data = new
            {
                user_id = userId,
                message,
                auto_escape = true,
            };
            var result = Utils.Post<SendPrivateMessageResponseData>(PrivateUrl, data);
            return result;
        }

        /// <summary>
        /// 发送群消息
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="message">要发送的内容（文本）</param>
        /// <returns></returns>
        public SendGroupMessageResponseData SendGroupMessage(long groupId, string message)
        {
            var data = new
            {
                group_id = groupId,
                message,
                auto_escape = true,
            };
            var result = Utils.Post<SendGroupMessageResponseData>(GroupUrl, data);
            return result;
        }

        /// <summary>
        /// 发送讨论组消息
        /// </summary>
        /// <param name="discussId">讨论组 ID</param>
        /// <param name="message">要发送的内容（文本）</param>
        /// <returns></returns>
        public SendDiscussMessageResponseData SendDiscussMessage(long discussId, string message)
        {
            var data = new
            {
                discuss_id = discussId,
                message,
                auto_escape = true,
            };
            var result = Utils.Post<SendDiscussMessageResponseData>(DiscussUrl, data);
            return result;
        }

        /// <summary>
        /// 群组踢人。
        /// </summary>
        /// <param name="groupId">群号。</param>
        /// <param name="userId">要踢的 QQ 号。</param>
        /// <returns>是否成功。注意：酷 Q 未处理错误，所以无论是否成功都会返回<c>true</c>。</returns>
        public bool KickGroupMember(long groupId, long userId)
        {
            var data = new
            {
                group_id = groupId,
                user_id = userId,
            };
            var success = Utils.Post<object>(KickGroupMemberUrl, data, out _);
            return success;
        }

        /// <summary>
        /// 撤回消息（需要Pro）
        /// </summary>
        /// <param name="message">消息返回值</param>
        /// <returns></returns>
        public bool RecallMessage(SendMessageResponseData message)
        {
            return RecallMessage(message.MessageId);
        }

        /// <summary>
        /// 撤回消息（需要Pro）
        /// </summary>
        /// <param name="messageId">消息返回值</param>
        /// <returns></returns>
        public bool RecallMessage(long messageId)
        {
            var data = new { message_id = (int)messageId };
            var success = Utils.Post<object>(RecallUrl, data, out _);
            return success;
        }
    }
}
