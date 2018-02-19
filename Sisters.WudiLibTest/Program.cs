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
            var privateResponse = httpApi.SendPrivateMessage(962549599, "hello");
            Console.WriteLine(privateResponse.MessageId);
            var groupResponse = httpApi.SendGroupMessage(72318078, "hello");
            Console.WriteLine(groupResponse.MessageId);
            //605617685
            #region kick test
            //var success1 = httpApi.KickGroupMember(605617685, 962549599);
            //var success2 = httpApi.KickGroupMember(72318078, 962549599);
            //Console.WriteLine(success1);
            //Console.WriteLine(success2);
            #endregion
            Console.WriteLine("--------------");
            #region recall test
            var delete1 = httpApi.RecallMessage(privateResponse);
            var delete2 = httpApi.RecallMessage(groupResponse);
            Console.WriteLine(delete1);
            Console.WriteLine(delete2);
            #endregion
            Console.ReadKey();
        }
    }
}
