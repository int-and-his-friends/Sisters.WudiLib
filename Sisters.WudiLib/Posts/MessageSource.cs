using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 表示消息发送人。
    /// </summary>
    public class MessageSource
    {
        protected MessageSource()
        {

        }

        public long UserId { get; protected set; }

    }

    public class PrivateMessageSource : MessageSource
    {
        internal PrivateMessageSource(long userId) => this.UserId = userId;
    }

    public class GroupMessageSource : MessageSource
    {
        public long GroupId { get; protected set; }

        internal GroupMessageSource(GroupMessage groupMessage)
        {
            this.UserId = groupMessage.UserId;
            this.GroupId = groupMessage.GroupId;
        }
    }
}
