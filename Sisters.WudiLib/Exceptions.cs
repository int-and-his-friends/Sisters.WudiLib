using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 访问 API 时出现的异常，例如，通过网络访问 API 失败。
    /// </summary>
    public class ApiAccessException : Exception
    {
        /// 
        public ApiAccessException()
        {

        }

        /// 
        public ApiAccessException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
