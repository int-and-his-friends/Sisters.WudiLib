using System;
using System.Threading.Tasks;

namespace Sisters.WudiLib.Api
{
    /// <summary>
    /// CoolQ HTTP API 插件接口客户端。
    /// </summary>
    public interface ICqHttpClient
    {
        /// <summary>
        /// 调用指定 API 功能。一般由 <see cref="CqHttpClientExtensions"/> 中的扩展方法调用，无需手动调用。
        /// </summary>
        /// <param name="action">要调用的 API 功能。例如 "send_group_msg"。</param>
        /// <param name="args">参数。</param>
        /// <returns>返回的 JSON 字符串。</returns>
        /// <exception cref="ArgumentNullException"><c>args</c> 为 <c>null</c>。</exception>
        /// <exception cref="Exception">可以尽情抛出任何异常。</exception>
        Task<string> CallAsync(string action, object args);
    }
}
