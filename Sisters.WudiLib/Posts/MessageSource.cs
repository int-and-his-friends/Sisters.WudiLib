using System;
using System.Collections.Generic;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 表示消息发送人。可能是普通来源或匿名来源。
    /// </summary>
    public sealed class MessageSource : IEquatable<MessageSource>
    {
        internal MessageSource(long userId, string anonymousFlag = null, string anonymous = null,
            bool isAnonymous = false)
        {
            IsAnonymous = isAnonymous;
            if (IsAnonymous)
            {
                Anonymous = anonymous;
                AnonymousFlag = anonymousFlag;
            }
            else
            {
                UserId = userId;
            }
        }

        /// <summary>
        /// 消息发送者是否匿名。
        /// </summary>
        public bool IsAnonymous { get; }

        /// <summary>匿名用户名称。</summary>
        public string Anonymous { get; }

        /// <summary>匿名用户 flag，在调用禁言 API 时需要传入。</summary>
        public string AnonymousFlag { get; }

        /// <summary>
        /// 发送者 QQ 号。
        /// </summary>
        public long UserId { get; }

        /// 
        public override bool Equals(object obj) => this.Equals(obj as MessageSource);

        /// 
        public bool Equals(MessageSource other) => other != null && this.IsAnonymous == other.IsAnonymous &&
                                                   this.Anonymous == other.Anonymous &&
                                                   this.AnonymousFlag == other.AnonymousFlag &&
                                                   this.UserId == other.UserId;

        /// 
        public override int GetHashCode()
        {
            var hashCode = -26995021;
            hashCode = hashCode * -1521134295 + this.IsAnonymous.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Anonymous);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.AnonymousFlag);
            hashCode = hashCode * -1521134295 + this.UserId.GetHashCode();
            return hashCode;
        }

        /// 
        public static bool operator ==(MessageSource source1, MessageSource source2) =>
            EqualityComparer<MessageSource>.Default.Equals(source1, source2);

        /// 
        public static bool operator !=(MessageSource source1, MessageSource source2) => !(source1 == source2);
    }
}
