using Sisters.WudiLib;
using System;

namespace Sisters.WudiLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpApi = new CoolQHttpApi();
            httpApi.ApiAddress = "http://127.0.0.7:5700/";
            httpApi.SendPrivateMessage(962549599, "hello");
        }
    }
}
