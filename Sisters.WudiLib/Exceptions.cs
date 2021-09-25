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

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiAccessException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ApiAccessException(string message)
            : base(message)
        {

        }

        /// 
        public ApiAccessException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
