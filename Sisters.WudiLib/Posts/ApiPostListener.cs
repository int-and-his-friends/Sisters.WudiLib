using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using http = System.Net.Http;

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
                if (!address.EndsWith("/")) address += "/";
                _postAddress = address;
            }
        }

        private string _forwardTo;
        /// <summary>
        /// 获取或设置转发地址。
        /// </summary>
        public string ForwardTo
        {
            get => _forwardTo;
            set => System.Threading.Interlocked.Exchange(ref _forwardTo, value);
        }

        private readonly object listenerLock = new object();

        private HttpListener listener = new HttpListener();

        private Task listenTask;

        /// <summary>
        /// 获取当前是否监听 HTTP API 的上报数据。
        /// </summary>
        public bool IsListening => listener.IsListening;

        public void StartListen()
        {
            lock (listenerLock)
            {
                if (IsListening) return;
                string prefix = PostAddress;
                listener.Prefixes.Add(prefix);
                listener.Start();
                listenTask = Task.Run((Action)Listening);
            }
        }

        private void Listening()
        {
            while (true)
            {
                var context = listener.GetContext();
                ProcessContext(context);
            }
        }

        private async void ProcessContext(HttpListenerContext context)
        {
            try
            {
                await Task.Run(() =>
                {
                    var request = context.Request;
                    using (var response = context.Response)
                    {
                        if (!request.ContentType.StartsWith("application/json")) return;

                        object responseObject;
                        string requestContent;

                        using (var inStream = request.InputStream)
                        using (var streamReader = new StreamReader(inStream))
                            requestContent = streamReader.ReadToEnd();

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
                LogException(e);
            }
        }

        private async void ForwardAsync(string content)
        {
            string to = ForwardTo;
            if (string.IsNullOrEmpty(to)) return;
            await Task.Run(async () =>
            {
                try
                {
                    using (var client = new http::HttpClient())
                    {
                        var stringContent = new http::StringContent(content, System.Text.Encoding.UTF8, "application/json");
                        using (await client.PostAsync(to, stringContent)) { }
                    }
                }
                catch (Exception e)
                {
                    OnException(e);
                }
            });
        }
        #endregion

        #region Logging
        public event Action<Exception> OnException;

        private void LogException(Exception e)
        {
            try
            {
                OnException(e);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region ProcessPost
        private object ProcessPost(string content, HttpListenerResponse response)
        {
            if (string.IsNullOrEmpty(content)) return null;

            GroupMessage post = JsonConvert.DeserializeObject<GroupMessage>(content);
            if (post == null) return null;

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
                    // TODO: input code
                    break;
                case Notice.GroupAdminNotice:
                    break;
                case Notice.GroupDecreaseNotice:
                    break;
                case Notice.GroupIncreaseNotice:
                    break;
                case Notice.FriendAddNotice:
                    break;
                default:
                    // TODO: Logging
                    break;
            }
        }

        private void ProcessGroupAdminNotice(JObject contentObject)
        {
            switch (contentObject[Post.SubTypeField].ToObject<string>())
            {
                case GroupAdminNotice.SetAdmin:
                    break;
                case GroupAdminNotice.UnsetAdmin:
                    break;
                default:
                    break;
            }
        }


        #endregion

        #region GroupRequest
        private readonly ICollection<GroupRequestEventHandler> groupRequestEventHandlers = new LinkedList<GroupRequestEventHandler>();

        /// <summary>
        /// 收到加群请求事件。
        /// </summary>
        public event GroupRequestEventHandler GroupRequestEvent
        {
            add { groupRequestEventHandlers.Add(value); }
            remove { groupRequestEventHandlers.Remove(value); }
        }

        private GroupRequestResponse GroupRequestHappen(GroupRequest request)
        {
            foreach (var handler in groupRequestEventHandlers)
            {
                var response = handler.Invoke(ApiClient, request);
                if (response != null) return response;
            }
            return null;
        }
        #endregion

        #region GroupInvite
        private readonly ICollection<GroupRequestEventHandler> groupInviteEventHandlers = new LinkedList<GroupRequestEventHandler>();

        /// <summary>
        /// 收到加群邀请事件。
        /// </summary>
        public event GroupRequestEventHandler GroupInviteEvent
        {
            add { groupInviteEventHandlers.Add(value); }
            remove { groupInviteEventHandlers.Remove(value); }
        }

        private GroupRequestResponse GroupInviteHappen(GroupRequest request)
        {
            foreach (var handler in groupInviteEventHandlers)
            {
                var response = handler.Invoke(ApiClient, request);
                if (response != null) return response;
            }
            return null;
        }
        #endregion

        #region FriendRequest
        private readonly ICollection<FriendRequestEventHandler> friendRequestEventHandlers = new LinkedList<FriendRequestEventHandler>();

        /// <summary>
        /// 收到好友请求事件。
        /// </summary>
        public event FriendRequestEventHandler FriendRequestEvent
        {
            add { friendRequestEventHandlers.Add(value); }
            remove { friendRequestEventHandlers.Remove(value); }
        }

        private FriendRequestResponse FriendRequestHappen(FriendRequest request)
        {
            foreach (var handler in friendRequestEventHandlers)
            {
                var response = handler.Invoke(ApiClient, request);
                if (response != null) return response;
            }
            return null;
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
