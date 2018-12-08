namespace Sisters.WudiLib
{
    internal static class MessageEscapingExtensions
    {

        /// <summary>
        /// 转义（编码）。
        /// </summary>
        /// <param name="before">要编码的字符串。</param>
        /// <param name="isCqCodeArg">是否是CQ码。如果为 <c>true</c>，也会转义逗号（<c>,</c>）；否则不会转义逗号。</param>
        /// <returns>转义结果。</returns>
        internal static string BeforeSend(this string before, bool isCqCodeArg)
        {
            var result = before
                .Replace("&", "&amp;")
                .Replace("[", "&#91;")
                .Replace("]", "&#93;");
            if (isCqCodeArg)
                result = result.Replace(",", "&#44;");
            return result;
        }

        /// <summary>
        /// 反转义（解码）。
        /// </summary>
        /// <param name="received">要解码的字符串。</param>
        /// <returns>解码结果。</returns>
        internal static string AfterReceive(this string received)
            => received
                .Replace("&#44;", ",")
                .Replace("&#91;", "[")
                .Replace("&#93;", "]")
                .Replace("&amp;", "&");
    }
}
