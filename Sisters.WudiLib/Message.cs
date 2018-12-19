namespace Sisters.WudiLib
{
    /// <summary>
    /// 各种消息类型的基类。
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// 构造 <see cref="Message"/> 实例必须运行的方法。
        /// </summary>
        protected Message()
        {
        }

        /// <summary>
        /// 返回发送时要序列化的对象。
        /// </summary>
        protected internal abstract object Serializing { get; }

        /// <summary>
        /// 用字符串表示的原始消息。
        /// </summary>
        public abstract string Raw { get; }
    }
}