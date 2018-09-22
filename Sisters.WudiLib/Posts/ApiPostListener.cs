using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.Posts
{
    public class ApiPostListener
    {
        #region

        /// <summary>
        /// 获取或设置 HTTP API 客户端实例，将在发生事件时传给事件处理器。
        /// </summary>
        public HttpApiClient ApiClient { get; set; }

        private string _postAddress;

        /// <summary>
        /// 获取或设置 HTTP API 的上报地址。如果已经开始监听，则设置无效。
        /// </summary>
        public string PostAddress
        {
            get => _postAddress;
            set
            {
                string address = value;
                if (!address.EndsWith("/"))
                    address += "/";
                _postAddress = address;
            }
        }

        /// <summary>
        /// 获取或设置转发地址。
        /// </summary>
        public string ForwardTo { get; set; }

        private byte[] _secretBytes;

        /// <summary>
        /// 设置 secret。用于验证上报数据是否来自插件。详见插件配置。
        /// </summary>
        /// <param name="secret">配置中的 secret 字段。</param>
        public void SetSecret(string secret) => _secretBytes = Encoding.UTF8.GetBytes(secret);

        private readonly object _listenerLock = new object();

        private readonly HttpListener _listener = new HttpListener();

        private Task _listenTask;

        /// <summary>
        /// 获取当前是否监听 HTTP API 的上报数据。
        /// </summary>
        public bool IsListening => _listener.IsListening;

        public ApiPostListener()
        {
        }

        public ApiPostListener(string address) => PostAddress = address;

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ApiPostListener(int port)
        {
            if (port <= IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException();
            PostAddress = $"http://+:{port.ToString(System.Globalization.CultureInfo.InvariantCulture)}/";
        }

        public void StartListen()
        {
            lock (_listenerLock)
            {
                if (IsListening)
                    return;
                string prefix = PostAddress;
                _listener.Prefixes.Add(prefix);
                _listener.Start();
                _listenTask = Task.Run((Action)Listening);
            }
        }

        private void Listening()
        {
            while (true)
            {
                var context = _listener.GetContext();
                ProcessContext(context);
            }
        }

        private async void ProcessContext(HttpListenerContext context)
        {
            string requestContent = null;
            try
            {
                await Task.Run(() =>
                {
                    var request = context.Request;
                    using (var response = context.Response)
                    {
                        if (!request.ContentType.StartsWith("application/json", StringComparison.Ordinal))
                            return;

                        object responseObject;

                        requestContent = GetContent(request);
                        if (string.IsNullOrEmpty(requestContent))
                            return;

                        // 转发
                        ForwardAsync(requestContent);

                        // 响应
                        responseObject = ProcessPost(requestContent, response);

                        response.ContentType = "application/json";
                        if (responseObject != null)
                        {
                            using (var outStream = response.OutputStream)
                            using (var streamWriter = new StreamWriter(outStream))
                            {
                                string jsonResponse = JsonConvert.SerializeObject(responseObject);
                                streamWriter.Write(jsonResponse);
                            }
                        }
                        else
                        {
                            response.StatusCode = 204;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                LogException(e, requestContent);
            }
        }

        private string GetContent(HttpListenerRequest request)
        {
            var length = request.ContentLength64;
            byte[] bytes = new byte[length * 2];
            int actualLength;
            using (request.InputStream)
                actualLength = request.InputStream.Read(bytes, 0, bytes.Length);

            // 验证
            var signature = request.Headers.Get("X-Signature");

            if (Verify(_secretBytes, signature, bytes, 0, actualLength))
            {
                string requestContent;
                requestContent = request.ContentEncoding.GetString(bytes, 0, actualLength);
                return requestContent;
            }

            // Authentication failed
            return null;
        }

        private static bool Verify(byte[] secret, string signature, byte[] buffer, int offset, int length)
        {
            if (secret == null)
                return true;
            using (var hmac = new HMACSHA1(secret))
            {
                hmac.Initialize();
                string result = BitConverter.ToString(hmac.ComputeHash(buffer, offset, length)).Replace("-", "");
                return string.Equals(signature, $"sha1={result}", StringComparison.OrdinalIgnoreCase);
            }
        }

        private async void ForwardAsync(string content)
        {
            string to = ForwardTo;
            if (string.IsNullOrEmpty(to))
                return;
            try
            {
                using (var client = new HttpClient())
                {
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    using (await client.PostAsync(to, stringContent))
                    {
                        // ignored
                    }
                }
            }
            catch (Exception)
            {
                //OnException(e);
            }
        }

        #endregion

        #region Logging

        public event Action<Exception> OnException;
        public event Action<Exception, string> OnExceptionWithRawContent;

        private void LogException(Exception e, string content)
        {
            try
            {
                OnException?.Invoke(e);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                OnExceptionWithRawContent?.Invoke(e, content);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region ProcessPost

        private object ProcessPost(string content, HttpListenerResponse response)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            GroupMessage post = JsonConvert.DeserializeObject<GroupMessage>(content);
            if (post == null)
                return null;

            switch (post.PostType)
            {
                case Post.MessagePost:
                    ProcessMessage(content, post);
                    return null;
                case Post.NoticePost:
                    JObject contentObject = JsonConvert.DeserializeObject<JObject>(content);
                    ProcessNotice(contentObject);
                    return null;
                case Post.RequestPost:
                    return ProcessRequest(content);
            }

            // log needed
            return null;
        }

        private void ProcessMessage(string content, GroupMessage groupMessage)
        {
            switch (groupMessage.MessageType)
            {
                case Message.PrivateType:
                    MessageEvent?.Invoke(ApiClient, JsonConvert.DeserializeObject<PrivateMessage>(content));
                    break;
                case Message.GroupType:
                    ProcessGroupMessage(content, groupMessage);
                    break;
                case Message.DiscussType:
                    MessageEvent?.Invoke(ApiClient, JsonConvert.DeserializeObject<DiscussMessage>(content));
                    break;
                default:
                    // log needed
                    break;
            }
        }

        private void ProcessGroupMessage(string content, GroupMessage groupMessage)
        {
            switch (groupMessage.SubType)
            {
                case GroupMessage.NormalType:
                    MessageEvent?.Invoke(ApiClient, groupMessage);
                    break;
                case GroupMessage.AnonymousType:
                    AnonymousMessageEvent?.Invoke(
                        ApiClient,
                        JsonConvert.DeserializeObject<AnonymousMessage>(content)
                    );
                    break;
                case GroupMessage.NoticeType:
                    GroupNoticeEvent?.Invoke(ApiClient, groupMessage);
                    break;
                default:
                    // log needed
                    break;
            }
        }

        private object ProcessRequest(string content)
        {
            GroupRequest request = JsonConvert.DeserializeObject<GroupRequest>(content);
            switch (request.RequestType)
            {
                case Request.FriendType:
                    return ProcessFriendRequest(request);
                case Request.GroupType:
                    return ProcessGroupRequest(request);
            }

            return null;
        }

        private object ProcessFriendRequest(FriendRequest friendRequest) => FriendRequestHappen(friendRequest);

        private object ProcessGroupRequest(GroupRequest groupRequest)
        {
            switch (groupRequest.SubType)
            {
                case GroupRequest.AddType:
                    return GroupRequestHappen(groupRequest);
                case GroupRequest.InvateType:
                    return GroupInviteHappen(groupRequest);
            }

            return null;
        }

        #endregion

        #region Notice

        private void ProcessNotice(JObject contentObject)
        {
            switch (contentObject[Notice.NoticeField].ToObject<string>())
            {
                case Notice.GroupUploadNotice:
                    GroupFileUploadedEvent?.Invoke(ApiClient, contentObject.ToObject<GroupFileNotice>());
                    break;
                case Notice.GroupAdminNotice:
                    ProcessGroupAdminNotice(contentObject);
                    break;
                case Notice.GroupDecreaseNotice:
                    ProcessGroupMemberDecrease(contentObject);
                    break;
                case Notice.GroupIncreaseNotice:
                    ProcessGroupMemberIncrease(contentObject);
                    break;
                case Notice.FriendAddNotice:
                    FriendAddedEvent?.Invoke(ApiClient, contentObject.ToObject<FriendAddNotice>());
                    break;
                default:
                    // TODO: Logging
                    break;
            }
        }

        private void ProcessGroupAdminNotice(JObject contentObject)
        {
            var data = contentObject.ToObject<GroupAdminNotice>();
            switch (data.SubType)
            {
                case GroupAdminNotice.SetAdmin:
                    GroupAdminSetEvent?.Invoke(ApiClient, data);
                    break;
                case GroupAdminNotice.UnsetAdmin:
                    GroupAdminUnsetEvent?.Invoke(ApiClient, data);
                    break;
                default:
                    // TODO
                    break;
            }
        }

        private void ProcessGroupMemberDecrease(JObject contentObject)
        {
            switch (contentObject[Post.SubTypeField].ToObject<string>())
            {
                case KickedNotice.Kicked:
                    KickedEvent?.Invoke(ApiClient, contentObject.ToObject<KickedNotice>());
                    break;
                default:
                    GroupMemberDecreasedEvent?.Invoke(ApiClient, contentObject.ToObject<GroupMemberDecreaseNotice>());
                    break;
            }
        }

        private void ProcessGroupMemberIncrease(JObject contentObject)
        {
            var data = contentObject.ToObject<GroupMemberIncreaseNotice>();
            if (data.IsMe)
            {
                GroupAddedEvent?.Invoke(ApiClient, data);
            }
            else
            {
                GroupMemberIncreasedEvent?.Invoke(ApiClient, data);
            }
        }

        public event Action<HttpApiClient, GroupFileNotice> GroupFileUploadedEvent;

        public event Action<HttpApiClient, GroupAdminNotice> GroupAdminSetEvent;

        public event Action<HttpApiClient, GroupAdminNotice> GroupAdminUnsetEvent;

        public event Action<HttpApiClient, FriendAddNotice> FriendAddedEvent;

        public event Action<HttpApiClient, GroupMemberDecreaseNotice> GroupMemberDecreasedEvent;

        public event Action<HttpApiClient, KickedNotice> KickedEvent;

        public event Action<HttpApiClient, GroupMemberIncreaseNotice> GroupMemberIncreasedEvent;

        /// <summary>
        /// 加入新群时发生的事件。注意此事件没有 <see cref="GroupMemberChangeNotice.OperatorId"/> 的数据（至少 Invite 没有，Approve 不清楚）。
        /// </summary>
        public event Action<HttpApiClient, GroupMemberIncreaseNotice> GroupAddedEvent;
        #endregion

        #region GroupRequest

        private readonly ICollection<GroupRequestEventHandler> _groupRequestEventHandlers =
            new LinkedList<GroupRequestEventHandler>();

        /// <summary>
        /// 收到加群请求事件。
        /// </summary>
        public event GroupRequestEventHandler GroupRequestEvent
        {
            add => _groupRequestEventHandlers.Add(value);
            remove => _groupRequestEventHandlers.Remove(value);
        }

        private GroupRequestResponse GroupRequestHappen(GroupRequest request)
        {
            return _groupRequestEventHandlers.Select(handler => handler.Invoke(ApiClient, request))
                .FirstOrDefault(response => response != null);
        }

        #endregion

        #region GroupInvite

        private readonly ICollection<GroupRequestEventHandler> _groupInviteEventHandlers =
            new LinkedList<GroupRequestEventHandler>();

        /// <summary>
        /// 收到加群邀请事件。此时 <see cref="Request.Comment"/> 并不存在。
        /// </summary>
        public event GroupRequestEventHandler GroupInviteEvent
        {
            add => _groupInviteEventHandlers.Add(value);
            remove => _groupInviteEventHandlers.Remove(value);
        }

        private GroupRequestResponse GroupInviteHappen(GroupRequest request)
        {
            return _groupInviteEventHandlers.Select(handler => handler.Invoke(ApiClient, request))
                .FirstOrDefault(response => response != null);
        }

        #endregion

        #region FriendRequest

        private readonly ICollection<FriendRequestEventHandler> _friendRequestEventHandlers =
            new LinkedList<FriendRequestEventHandler>();

        /// <summary>
        /// 收到好友请求事件。
        /// </summary>
        public event FriendRequestEventHandler FriendRequestEvent
        {
            add => _friendRequestEventHandlers.Add(value);
            remove => _friendRequestEventHandlers.Remove(value);
        }

        private FriendRequestResponse FriendRequestHappen(FriendRequest request)
        {
            return _friendRequestEventHandlers.Select(handler => handler.Invoke(ApiClient, request))
                .FirstOrDefault(response => response != null);
        }

        #endregion

        #region Message

        /// <summary>
        /// 收到消息事件。包括私聊、群聊和讨论组消息，但不包括匿名的群消息。
        /// </summary>
        public event MessageEventHandler MessageEvent;

        /// <summary>
        /// 收到匿名群消息事件。
        /// </summary>
        public event AnonymousMessageEventHanlder AnonymousMessageEvent;

        /// <summary>
        /// 群聊信息事件。例如禁言等。
        /// </summary>
        public event GroupNoticeEventHandler GroupNoticeEvent;

        #endregion

        #region DefaultHandlers

        /// <summary>
        /// 同意全部群组请求（请求、邀请）的事件处理器。
        /// </summary>
        /// <param name="api"></param>
        /// <param name="groupRequest"></param>
        /// <returns></returns>
        public static GroupRequestResponse ApproveAllGroupRequests(HttpApiClient api, GroupRequest groupRequest)
            => new GroupRequestResponse { Approve = true };

        /// <summary>
        /// 同意全部好友请求的事件处理器。
        /// </summary>
        /// <param name="api"></param>
        /// <param name="friendRequest"></param>
        /// <returns></returns>
        public static FriendRequestResponse ApproveAllFriendRequests(HttpApiClient api, FriendRequest friendRequest)
            => new FriendRequestResponse { Approve = true };

        /// <summary>
        /// 复读的事件处理器。并没有什么卵用。
        /// </summary>
        /// <param name="api"></param>
        /// <param name="message"></param>
        public static async void RepeatAsync(HttpApiClient api, Message message)
            => await api?.SendMessageAsync(message.Endpoint, message.Content);

        /// <summary>
        /// 当收到消息时，在同一处发送指定内容消息的事件处理器。并没有什么卵用。
        /// </summary>
        /// <param name="something"></param>
        /// <returns></returns>
        public static MessageEventHandler Say(string something)
            => async (api, message) => await api?.SendMessageAsync(message.Endpoint, something);

        //public static void Print(HttpApiClient api, GroupMessage notice)
        //    => Console.WriteLine(notice.Content);

        #endregion
    }
}
