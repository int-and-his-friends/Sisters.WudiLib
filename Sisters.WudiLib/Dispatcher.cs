using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib
{
#nullable enable
    /// <summary>
    /// Dispatches posts.
    /// </summary>
    internal class Dispatcher
    {
        private readonly ILogger<Dispatcher> _logger;
        private readonly HttpApiClient _onebotApi;
        private readonly ApiPostListener _onebotPost;
        /// <summary>
        /// 初始化一个 Dispatcher。
        /// </summary>
        /// <param name="onebotApi"></param>
        /// <param name="onebotPost"></param>
        /// <param name="logger">Logger。</param>
        public Dispatcher(HttpApiClient onebotApi, ApiPostListener onebotPost, ILogger<Dispatcher>? logger = null)
        {
            _logger = logger ?? NullLogger<Dispatcher>.Instance;
            _onebotApi = onebotApi;
            _onebotPost = onebotPost;
        }


    }
#nullable restore
}
