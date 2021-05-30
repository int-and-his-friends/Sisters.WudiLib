using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.Builders
{
#nullable enable
    /// <summary>
    /// 读取事件列表，构造 Dispatcher。
    /// </summary>
    internal class DispatcherBuilder
    {
#if NET45
        private readonly ILogger _logger;
#else
        private readonly ILogger<Dispatcher> _logger;
#endif
        /// <summary>
        /// 初始化一个 Dispatcher Builder。
        /// </summary>
        /// <param name="logger">Logger。</param>
        public DispatcherBuilder(ILogger<Dispatcher>? logger = null)
        {
#if NET45
            _logger = logger as ILogger ?? NullLogger.Instance;
#else
            _logger = logger ?? NullLogger<Dispatcher>.Instance;
#endif
        }

        /// <summary>
        /// 从程序集中搜索事件列表并添加。
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns>This.</returns>
        public DispatcherBuilder AddAssembly(Assembly assembly)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 添加事件类型。
        /// </summary>
        /// <typeparam name="T">要添加的类型。</typeparam>
        /// <returns>This.</returns>
        public DispatcherBuilder AddType<T>()
        {
            var type = typeof(T);
            throw new NotImplementedException();
        }

        /// <summary>
        /// 构建调配委托。
        /// </summary>
        /// <returns></returns>
        public Func<JObject, Post> BuildDispatchingDelegate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 构建 Dispatcher。
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public Dispatcher BuildDispatcher(ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Dispatcher>();
            throw new NotImplementedException();
        }
    }
#nullable restore
}
