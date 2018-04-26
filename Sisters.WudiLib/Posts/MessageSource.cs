namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 表示消息发送人。可能是普通来源或匿名来源。
    /// </summary>
    public class MessageSource
    {
        internal MessageSource(long userId, string anonymousFlag = null, string anonymous = null, bool isAnonymous = false)
        {
            _userId = userId;
            _isAnonymous = isAnonymous;
            _anonymous = anonymous;
            _anonymousFlag = anonymousFlag;
        }

        private readonly bool _isAnonymous;
        public bool IsAnonymous => _isAnonymous;

        private readonly string _anonymous;
        public string Anonymous => _anonymous;

        private readonly string _anonymousFlag;
        public string AnonymousFlag => _anonymousFlag;

        private readonly long _userId;
        public long UserId => _userId;
    }
}
