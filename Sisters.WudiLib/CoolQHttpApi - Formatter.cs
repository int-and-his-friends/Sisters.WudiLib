using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib
{
    public partial class CoolQHttpApi
    {
        public static class Formatter
        {
            public static string IMAGE_FORMAT => "[CQ:image,file={0}]";

            /// <summary>
            /// 生成图片 CQ 码
            /// </summary>
            /// <param name="param">图片名或其他支持的参数</param>
            /// <returns>CQ码</returns>
            public static string FormatImage(string param) => string.Format(Formatter.IMAGE_FORMAT, param);
        }
    }
}
