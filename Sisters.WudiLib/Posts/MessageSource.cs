using System;
using System.Collections.Generic;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 表示消息发送人。可能是普通来源或匿名来源。
    /// </summary>
    public sealed class MessageSource : IEquatable<MessageSource>
    {
        internal MessageSource(long userId, string anonymousFlag = null, string anonymous = null, bool isAnonymous = false)
        {
            _isAnonymous = isAnonymous;
            if (_isAnonymous)
            {
                _anonymous = anonymous;
                _anonymousFlag = anonymousFlag;
            }
            else
            {
                _userId = userId;
            }
        }

        private readonly bool _isAnonymous;
        public bool IsAnonymous => _isAnonymous;

        private readonly string _anonymous;
        public string Anonymous => _anonymous;

        private readonly string _anonymousFlag;
        public string AnonymousFlag => _anonymousFlag;

        private readonly long _userId;
        public long UserId => _userId;

        public override bool Equals(object obj) => this.Equals(obj as MessageSource);
        public bool Equals(MessageSource other) => other != null && this._isAnonymous == other._isAnonymous && this._anonymous == other._anonymous && this._anonymousFlag == other._anonymousFlag && this._userId == other._userId;

        public override int GetHashCode()
        {
            var hashCode = -26995021;
            hashCode = hashCode * -1521134295 + this._isAnonymous.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this._anonymous);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this._anonymousFlag);
            hashCode = hashCode * -1521134295 + this._userId.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MessageSource source1, MessageSource source2) => EqualityComparer<MessageSource>.Default.Equals(source1, source2);
        public static bool operator !=(MessageSource source1, MessageSource source2) => !(source1 == source2);
    }
}
