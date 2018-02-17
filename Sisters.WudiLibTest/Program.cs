using Sisters.WudiLib;
using System;

namespace Sisters.WudiLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpApi = new CoolQHttpApi();
            httpApi.ApiAddress = "http://127.0.0.1:5700/";
            httpApi.SendPrivateMessage(962549599, "hello");
            httpApi.SendGroupMessage(72318078, "hello");
            //605617685
            httpApi.KickGroupMember(605617685, 962549599);
        }
    }
}
