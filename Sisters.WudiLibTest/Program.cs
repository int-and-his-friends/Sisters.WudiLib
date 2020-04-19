using System;
using System.Globalization;
using System.Threading.Tasks;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Message = Sisters.WudiLib.SendingMessage;
using MessageContext = Sisters.WudiLib.Posts.Message;

namespace Sisters.WudiLibTest
{
    internal static class Program
    {
        private static void PrintNoticeAndRequests(ApiPostListener apiPostListener)
        {
            void PrintPost(Post post)
            {
                Console.WriteLine(post.Time);
                Console.WriteLine("user: " + post.UserId);
                Console.WriteLine("self: " + post.SelfId);
            }

            void PrintRequest(Request request)
            {
                PrintPost(request);
                Console.WriteLine(request.Comment);
                Console.WriteLine(request.Flag);
            }

            apiPostListener.GroupFileUploadedEvent += (api, notice) =>
            {
                Console.WriteLine("file uploaded");
                PrintPost(notice);
                Console.WriteLine("group: " + notice.GroupId);
                Console.WriteLine($"file: {notice.File.Id} | name: {notice.File.Name} | size: {notice.File.Length} | busid: {notice.File.BusId}");
            };

            apiPostListener.GroupAdminSetEvent += (api, notice) =>
            {
                Console.WriteLine($"admin set ({notice.SubType})");
                PrintPost(notice);
                Console.WriteLine($"group: {notice.GroupId}");
            };

            apiPostListener.GroupAdminUnsetEvent += (api, notice) =>
            {
                Console.WriteLine($"admin unset ({notice.SubType})");
                PrintPost(notice);
                Console.WriteLine($"group: {notice.GroupId}");
            };

            apiPostListener.GroupMemberDecreasedEvent += (api, notice) =>
            {
                Console.WriteLine($"group member decreased ({notice.SubType})");
                PrintPost(notice);
                Console.WriteLine($"group: {notice.GroupId}");
                Console.WriteLine($"operator: {notice.OperatorId}");
            };

            apiPostListener.KickedEvent += (api, notice) =>
            {
                Console.WriteLine($"kicked ({notice.SubType})");
                PrintPost(notice);
                Console.WriteLine($"group: {notice.GroupId}");
                Console.WriteLine($"operator: {notice.OperatorId}");
            };

            apiPostListener.GroupMemberIncreasedEvent += (api, notice) =>
            {
                Console.WriteLine($"group member increased ({notice.SubType})");
                PrintPost(notice);
                Console.WriteLine($"group: {notice.GroupId}");
                Console.WriteLine($"operator: {notice.OperatorId}");
            };

            apiPostListener.GroupAddedEvent += (api, notice) =>
            {
                Console.WriteLine($"group join ({notice.SubType})");
                PrintPost(notice);
                Console.WriteLine($"group: {notice.GroupId}");
                Console.WriteLine($"operator: {notice.OperatorId}");
            };

            apiPostListener.FriendAddedEvent += (api, notice) =>
            {
                Console.WriteLine("friend added");
                PrintPost(notice);
            };

            apiPostListener.FriendRequestEvent += (api, request) =>
            {
                Console.WriteLine("friend request");
                PrintRequest(request);
                return new FriendRequestResponse { Approve = false };
            };

            apiPostListener.GroupInviteEvent += (api, request) =>
            {
                Console.WriteLine($"group invite");
                PrintRequest(request);
                Console.WriteLine($"group: {request.GroupId}");
                if (request.UserId != 962549599)
                {
                    return new GroupRequestResponse { Approve = false };
                }
                else
                {
                    return new GroupRequestResponse { Approve = true };
                }
            };

            apiPostListener.GroupRequestEvent += (api, request) =>
            {
                Console.WriteLine($"group request");
                PrintRequest(request);
                Console.WriteLine($"group: {request.GroupId}");
                return new GroupRequestResponse { Approve = false };
            };
        }

        private static void Main(string[] args)
        {
            var culture = CultureInfo.GetCultureInfo("zh-CN");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            var httpApiClient = new HttpApiClient();

            var friendList = httpApiClient.GetFriendListAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            // test for get_friend_list ha kokomade.

            var groupMemberInfo = httpApiClient.GetGroupMemberInfoAsync(661021255, 962549599).GetAwaiter().GetResult();
            //var groupMemberInfos = httpApiClient.GetGroupMemberListAsync(641236878).GetAwaiter().GetResult();

            var banResult = httpApiClient.BanAnonymousMember(72318078, "AAAAAAAPQl8ADMfHwK2hpMSqtvvDyQAoAHXUxZeiC+YKi480g3ERUrpzM+o20KsUJ0mm1xxoobOEtwYU+3KqiA==", 60).GetAwaiter().GetResult();

            banResult = httpApiClient.BanWholeGroup(72318078, true).GetAwaiter().GetResult();
            Task.Delay(TimeSpan.FromSeconds(10)).Wait();
            banResult = httpApiClient.BanWholeGroup(72318078, false).GetAwaiter().GetResult();

            var postListener = new ApiPostListener(8876);
            PrintNoticeAndRequests(postListener);
            postListener.StartListen();
            Console.ReadKey(true);
            Environment.Exit(0);

            var httpApi = new HttpApiClient();
            httpApi.ApiAddress = "http://127.0.0.1:5700/";

            //var privateResponse = httpApi.SendPrivateMessageAsync(962549599, "hello").Result;
            //Console.WriteLine(privateResponse.MessageId);
            //var groupResponse = httpApi.SendGroupMessageAsync(72318078, "hello").Result;
            //Console.WriteLine(groupResponse.MessageId);
            //605617685
            #region kick test
            //var success1 = httpApi.KickGroupMember(605617685, 962549599);
            //var success2 = httpApi.KickGroupMember(72318078, 962549599);
            //Console.WriteLine(success1);
            //Console.WriteLine(success2);
            #endregion
            Console.WriteLine("--------------");
            #region recall test
            //var delete1 = httpApi.RecallMessageAsync(privateResponse).Result;
            //var delete2 = httpApi.RecallMessageAsync(groupResponse).Result;
            //Console.WriteLine(delete1);
            //Console.WriteLine(delete2);
            #endregion

            #region group member info test
            //Console.Write("group num:");
            //long group = long.Parse(Console.ReadLine().Trim());
            //Console.Write("qq id:");
            //long qqid = long.Parse(Console.ReadLine().Trim());
            //var member = httpApi.GetGroupMemberInfoAsync(group, qqid).Result;
            //Console.WriteLine(member.Age);
            //Console.WriteLine(member.Area);
            //Console.WriteLine(member.Authority.ToString());
            //Console.WriteLine(member.GroupId);
            //Console.WriteLine(member.InGroupName);
            //Console.WriteLine(member.IsCardChangeable);
            //Console.WriteLine(member.JoinTime);
            //Console.WriteLine(member.LastSendTime);
            //Console.WriteLine(member.Nickname);
            //Console.WriteLine(member.Title);
            //Console.WriteLine(member.UserId);

            //var memberList = httpApi.GetGroupMemberListAsync(605617685).Result;
            //var query = from m in memberList
            //            where m.Age > 19
            //            select new { m.InGroupName, m.Nickname, m.Area };
            //foreach (var item in query)
            //{
            //    Console.WriteLine(item.InGroupName);
            //    Console.WriteLine(item.Nickname);
            //    Console.WriteLine(item.Area);
            //}
            #endregion

            #region Message Class Test
            //var message = new Message("this is at test,: ");
            //message += Message.At(962549599);
            //httpApi.SendGroupMessageAsync(72318078, message).Wait();
            #endregion

            #region Image Test
            //var imgMessage = Message.LocalImage(@"C:\Users\Administrator\Desktop\Rinima.jpg");
            //var netMessage = Message.NetImage(@"https://files.yande.re/image/ca815083c96a99a44ff72e70c6957c14/yande.re%20437737%20dennou_shoujo_youtuber_shiro%20heels%20pantyhose%20shiro_%28dennou_shoujo_youtuber_shiro%29%20shouju_ling.jpg");
            //httpApi.SendGroupMessageAsync(72318078, imgMessage).Wait();
            //httpApi.SendGroupMessageAsync(72318078, netMessage).Wait();
            //httpApi.SendGroupMessageAsync(72318078, imgMessage + netMessage).Wait();
            //message += netMessage;
            //httpApi.SendGroupMessageAsync(72318078, message).Wait();
            #endregion

            RecordTestAsync(httpApi);

            //ListeningTest(httpApi);

            Console.WriteLine("end");
            Console.ReadKey();
        }

        private static async void RecordTestAsync(HttpApiClient httpApi)
        {
            var record = Message.NetRecord("https://b.ppy.sh/preview/758101.mp3");
            await httpApi.SendPrivateMessageAsync(962549599, record);
        }

        private static void ListeningTest(HttpApiClient httpApi)
        {
            Console.WriteLine("input listening port");
            string port = Console.ReadLine();

            ApiPostListener listener = new ApiPostListener();
            listener.ApiClient = httpApi;
            listener.PostAddress = $"http://127.0.0.1:{port}/";
            listener.ForwardTo = "http://[::1]:10202";
            listener.StartListen();
            listener.FriendRequestEvent += Friend;
            listener.FriendRequestEvent += ApiPostListener.ApproveAllFriendRequests;
            listener.GroupInviteEvent += Group;
            listener.GroupInviteEvent += ApiPostListener.ApproveAllGroupRequests;
            listener.GroupRequestEvent += Group;
            listener.GroupRequestEvent += ApiPostListener.ApproveAllGroupRequests;
            //listener.MessageEvent += ApiPostListener.RepeatAsync;
            listener.MessageEvent += PrintRaw;
            listener.GroupNoticeEvent += ApiPostListener.RepeatAsync;
            listener.AnonymousMessageEvent += ApiPostListener.RepeatAsync;
            listener.AnonymousMessageEvent += PrintRaw;
            //listener.MessageEvent += ApiPostListener.Say("good");

            var listener2 = new ApiPostListener();
            listener2.PostAddress = "http://[::1]:10202";
            listener2.StartListen();
            listener2.MessageEvent += (api, message) =>
            {
                Console.WriteLine(message.Time);
                Console.WriteLine(message.Content.Raw);
            };
        }

        private static void PrintRaw(HttpApiClient api, MessageContext message)
        {
            Console.WriteLine(message.Content.Raw);
        }

        private static GroupRequestResponse Group(HttpApiClient api, GroupRequest request)
        {
            Console.WriteLine("Group");
            Console.WriteLine(request.Time);
            Console.WriteLine(request.SelfId);
            Console.WriteLine(request.GroupId);
            Console.WriteLine(request.UserId);
            Console.WriteLine(request.Comment);
            Console.WriteLine(request.Flag);
            return null;
        }

        private static FriendRequestResponse Friend(HttpApiClient api, FriendRequest request)
        {
            Console.WriteLine("Friend");
            Console.WriteLine(request.Time);
            Console.WriteLine(request.SelfId);
            Console.WriteLine(request.UserId);
            Console.WriteLine(request.Comment);
            Console.WriteLine(request.Flag);
            return null;
        }
    }
}
