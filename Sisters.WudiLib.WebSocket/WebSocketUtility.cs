using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Sisters.WudiLib.WebSocket
{
    internal static class WebSocketUtility
    {
        internal static Uri CreateUri(string url, string accessToken)
        {
            var uriBuilder = new UriBuilder(url);
            if (!string.IsNullOrEmpty(accessToken))
            {
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["access_token"] = accessToken;
                /* 在.NET Framework 中，这里的 ToString 会编码成 %uxxxx 的格式，
                 * 现在已经不用这种格式了。.NET Core 可以正确处理。解决这个问题之后，
                 * 此类库应该可以用于 .NET Framework。
                 */
                uriBuilder.Query = query.ToString();
            }
            Uri uri = uriBuilder.Uri;
            return uri;
        }
    }
}
