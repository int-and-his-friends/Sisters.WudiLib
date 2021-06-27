using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib.Builders.Annotations;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.Builders
{
#nullable enable
    /// <summary>
    /// 读取事件列表，构造 Dispatcher。
    /// </summary>
    internal class DispatcherBuilder
    {
        private readonly ILogger<Dispatcher> _logger;
        private readonly PostTreeNode _root = new PostTreeNode(typeof(Post), false);

        /// <summary>
        /// 初始化一个 Dispatcher Builder。
        /// </summary>
        /// <param name="logger">Logger。</param>
        public DispatcherBuilder(ILogger<Dispatcher>? logger = null)
        {
            _logger = logger ?? NullLogger<Dispatcher>.Instance;
        }

        /// <summary>
        /// 从程序集中搜索事件列表并添加。
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns>This.</returns>
        public DispatcherBuilder AddAssembly(Assembly assembly) => AddAssemblyExcept(assembly);

        public DispatcherBuilder AddAssemblyExcept(Assembly assembly, params Type[] excludedTypes)
        {
            _logger.LogInformation($"添加程序集：{assembly.FullName}，跳过 {excludedTypes.Length} 个类型。");
            foreach (var t in assembly.GetTypes().Except(excludedTypes))
            {
                if (t.GetCustomAttributes<PostAttribute>().Any())
                {
                    if (!t.IsAbstract)
                    {
                        _logger.LogInformation($"发现 {t.FullName}，正在添加。");
                        _root.AddType(t);
                    }
                    else
                    {
                        _logger.LogInformation($"跳过抽象类 {t.FullName}。");
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// 添加事件类型。
        /// </summary>
        /// <typeparam name="T">要添加的类型。</typeparam>
        /// <returns>This.</returns>
        public DispatcherBuilder AddType<T>() => AddType(typeof(T));

        public DispatcherBuilder AddType(Type type)
        {
            _logger.LogInformation($"添加类型 {type.FullName}");
            _root.AddType(type);
            return this;
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
