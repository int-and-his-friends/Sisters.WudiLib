using Sisters.WudiLib;
using System;
using System.Linq;

namespace Sisters.WudiLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var httpApi = new CoolQHttpApi();
            httpApi.ApiAddress = "http://127.0.0.1:5700/";
            var privateResponse = httpApi.SendPrivateMessageAsync(962549599, "hello").Result;
            Console.WriteLine(privateResponse.MessageId);
            var groupResponse = httpApi.SendGroupMessageAsync(72318078, "hello").Result;
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
            var delete1 = httpApi.RecallMessageAsync(privateResponse).Result;
            var delete2 = httpApi.RecallMessageAsync(groupResponse).Result;
            Console.WriteLine(delete1);
            Console.WriteLine(delete2);
            #endregion

            #region group member info test
            Console.Write("group num:");
            long group = long.Parse(Console.ReadLine().Trim());
            Console.Write("qq id:");
            long qqid = long.Parse(Console.ReadLine().Trim());
            var member = httpApi.GetGroupMemberInfoAsync(group, qqid).Result;
            Console.WriteLine(member.Age);
            Console.WriteLine(member.Area);
            Console.WriteLine(member.Authority.ToString());
            Console.WriteLine(member.GroupId);
            Console.WriteLine(member.InGroupName);
            Console.WriteLine(member.IsCardChangeable);
            Console.WriteLine(member.JoinTime);
            Console.WriteLine(member.LastSendTime);
            Console.WriteLine(member.Nickname);
            Console.WriteLine(member.Title);
            Console.WriteLine(member.UserId);

            var memberList = httpApi.GetGroupMemberListAsync(605617685).Result;
            var query = from m in memberList
                        where m.Age > 19
                        select new { m.InGroupName, m.Nickname, m.Area };
            foreach (var item in query)
            {
                Console.WriteLine(item.InGroupName);
                Console.WriteLine(item.Nickname);
                Console.WriteLine(item.Area);
            }
            #endregion
            Console.ReadKey();
        }
    }
}
