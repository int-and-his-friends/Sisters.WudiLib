using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.Posts
{
    /// <summary>
    /// 上报数据监听器。
    /// </summary>
    public partial class ApiPostListener
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
        public virtual string PostAddress
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
        public virtual bool IsListening => _listener.IsListening;

        /// 
        public ApiPostListener()
        {
        }

        /// <summary>
        /// 通过上报地址构造 <see cref="ApiPostListener"/> 实例。
        /// </summary>
        /// <param name="address">要监听的上报地址。</param>
        public ApiPostListener(string address) => PostAddress = address;

        /// <summary>通过监听端口构造 <see cref="ApiPostListener"/> 实例。所有发往该端口的数据都将被监听。</summary>
        /// <exception cref="ArgumentOutOfRangeException"><c>port</c> 小于等于 <see
        /// cref="IPEndPoint.MinPort"/>，或者大于 <see cref="IPEndPoint.MaxPort"/>。</exception>
        public ApiPostListener(int port)
        {
            if (port <= IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException();
            PostAddress = $"http://+:{port.ToString(System.Globalization.CultureInfo.InvariantCulture)}/";
        }

        /// <summary>
        /// 开始监听上报。
        /// </summary>
        /// <exception cref="Exception">启动时出现错误。</exception>
        public virtual void StartListen()
        {
            lock (_listenerLock)
            {
                if (IsListening)
                    return;
                string prefix = PostAddress;
                _listener.Prefixes.Add(prefix);
                _listener.Start();
                _listenTask = ListeningAsync();
            }
        }

        private async Task ListeningAsync()
        {
            while (true)
            {
                var context = await _listener.GetContextAsync().ConfigureAwait(false);
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

                        requestContent = GetContentAndForward(request);
                        if (string.IsNullOrEmpty(requestContent))
                            return;

                        // 响应
                        responseObject = ProcessPost(requestContent);

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
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogException(e, requestContent);
            }
        }

        /// <summary>
        /// 从请求中读取内容。
        /// </summary>
        /// <param name="request">收到的 Http 请求。</param>
        /// <returns>读取到的内容。</returns>
        private string GetContentAndForward(HttpListenerRequest request)
        {
            byte[] bytes = new byte[request.ContentLength64];
            using (var ms = new MemoryStream(bytes))
            {
                request.InputStream.CopyTo(ms);
            }
            (request.InputStream as IDisposable).Dispose();

            // 验证
            var signature = request.Headers.Get("X-Signature");

            if (Verify(_secretBytes, signature, bytes, 0, bytes.Length))
            {
                string requestContent;
                requestContent = request.ContentEncoding.GetString(bytes);

                // 转发
                ForwardAsync(bytes, request.ContentEncoding, signature);

                return requestContent;
            }

            // Authentication failed
            return null;
        }

        /// <summary>
        /// Secret 为 <c>null</c> 时直接返回 <c>true</c>。
        /// </summary>
        private static bool Verify(byte[] secret, string signature, byte[] buffer, int offset, int length)
        {
            if (secret is null)
                return true;
            if (signature is null)
                return false;
            using (var hmac = new HMACSHA1(secret))
            {
                hmac.Initialize();
                string result = BitConverter.ToString(hmac.ComputeHash(buffer, offset, length)).Replace("-", "");
                return string.Equals(signature, $"sha1={result}", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 异步通过 HTTP 转发上报事件。转发失败时不会有任何提示。
        /// </summary>
        /// <param name="content">转发内容。</param>
        /// <param name="signature">HTTP 头部的签名。</param>
        [Obsolete]
        protected void ForwardAsync(string content, string signature)
        {
            string to = ForwardTo;
            if (string.IsNullOrEmpty(to))
                return;

            ForwardAsync(Encoding.UTF8.GetBytes(content), Encoding.UTF8, signature);
        }

        /// <summary>
        /// 异步通过 HTTP 转发上报事件。转发失败时不会有任何提示。
        /// </summary>
        /// <param name="content">转发内容。</param>
        /// <param name="encoding">字符编码。</param>
        /// <param name="signature">HTTP 头部的签名。</param>
        protected async void ForwardAsync(byte[] content, Encoding encoding, string signature)
        {
            string to = ForwardTo;
            if (string.IsNullOrEmpty(to))
                return;
            try
            {
                using (var client = new HttpClient())
                {
                    if (signature != null)
                    {
                        client.DefaultRequestHeaders.Add("X-Signature", signature);
                    }

                    var byteArrayContent = new ByteArrayContent(content);
                    var headerValue = new MediaTypeHeaderValue("application/json");
                    headerValue.CharSet = encoding.WebName;

                    byteArrayContent.Headers.ContentType = headerValue;
                    using (await client.PostAsync(to, byteArrayContent).ConfigureAwait(false))
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

        /// <summary>
        /// 处理上报中发生了异常。可能是业务逻辑中的异常，也可能是数据传输或解析过程中的异常。
        /// </summary>
        public event Action<Exception> OnException;

        /// <summary>
        /// 处理上报中发生了异常。可能是业务逻辑中的异常，也可能是数据传输或解析过程中的异常。此事件会包含上报的原始数据。
        /// </summary>
        public event Action<Exception, string> OnExceptionWithRawContent;

        /// <summary>
        /// 触发 <see cref="OnException"/> 和 <see cref="OnExceptionWithRawContent"/> 事件。
        /// </summary>
        /// <param name="e">异常。</param>
        /// <param name="content">上报内容。</param>
        protected void LogException(Exception e, string content)
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

        /// <summary>
        /// 传入收到的 JSON 上报数据，调用处理器处理。
        /// </summary>
        /// <param name="content">收到的 JSON 数据。</param>
        /// <returns>由处理器返回的数据。</returns>
        /// <exception cref="Exception">处理时发生异常。</exception>
        public Response ProcessPost(string content)
        {
            /*Post p = GetPost(content);
            if (p is null)
                return null;
            if (p is Message)
            {
            }
            else if (p is Notice)
            {
            }
            else if (p is Request)
            {
            }*///这样？

            if (string.IsNullOrEmpty(content))
                return null;

            JObject contentObject = JsonConvert.DeserializeObject<JObject>(content);
            return ProcessPost(contentObject);
        }

        /// <summary>
        /// 传入上报事件的 <see cref="JObject"/>，调用处理器处理。
        /// </summary>
        /// <param name="contentObject">上报事件对应的 <see cref="JObject"/>。</param>
        /// <returns>响应。</returns>
        /// <exception cref="Exception">处理时发生异常。</exception>
        public virtual Response ProcessPost(JObject contentObject)
        {
            if (contentObject is null)
                return null;

            try
            {
                EventPosted?.Invoke(contentObject);
            }
            catch (Exception e)
            {
                OnException?.Invoke(e);
            }

            switch (contentObject[Post.TypeField].ToObject<string>())
            {
                case Post.Message:
                    ProcessMessage(contentObject);
                    return null;
                case Post.Notice:
                    ProcessNotice(contentObject);
                    return null;
                case Post.Request:
                    return ProcessRequest(contentObject);
            }

            // log needed
            return null;
        }

        protected virtual void ProcessMessage(JObject contentObject)
        {
            GroupMessage groupMessage = contentObject.ToObject<GroupMessage>();
            switch (groupMessage.MessageType)
            {
                case Message.PrivateType:
                    MessageEvent?.Invoke(ApiClient, contentObject.ToObject<PrivateMessage>());
                    break;
                case Message.GroupType:
                    ProcessGroupMessage(contentObject, groupMessage);
                    break;
                case Message.DiscussType:
                    MessageEvent?.Invoke(ApiClient, contentObject.ToObject<DiscussMessage>());
                    break;
                default:
                    // log needed
                    break;
            }
        }

        protected virtual void ProcessGroupMessage(JObject contentObject, GroupMessage groupMessage)
        {
            switch (groupMessage.SubType)
            {
                case GroupMessage.NormalType:
                    MessageEvent?.Invoke(ApiClient, groupMessage);
                    break;
                case GroupMessage.AnonymousType:
                    AnonymousMessageEvent?.Invoke(
                        ApiClient,
                        contentObject.ToObject<AnonymousMessage>()
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

        private RequestResponse ProcessRequest(JObject jObject)
        {
            switch (jObject[Request.TypeField].ToObject<string>())
            {
                case Request.Friend:
                    return ProcessFriendRequest(jObject.ToObject<FriendRequest>());
                case Request.Group:
                    return ProcessGroupRequest(jObject.ToObject<GroupRequest>());
            }

            return null;
        }

        private RequestResponse ProcessFriendRequest(FriendRequest friendRequest) => FriendRequestHappen(friendRequest);

        private RequestResponse ProcessGroupRequest(GroupRequest groupRequest)
        {
            switch (groupRequest.SubType)
            {
                case GroupRequest.Add:
                    return GroupRequestHappen(groupRequest);
                case GroupRequest.Invite:
                    return GroupInviteHappen(groupRequest);
            }

            return null;
        }

        #endregion

        #region Event

        /// <summary>
        /// 当有任意的事件上报到达时触发。此事件只应该被 SDK 注册，用于扩展，并且此事件执行时不应阻塞。
        /// </summary>
        public event Action<JObject> EventPosted;

        #endregion

        #region Notice

        protected virtual void ProcessNotice(JObject contentObject)
        {
            switch (contentObject[Notice.TypeField].ToObject<string>())
            {
                case Notice.GroupUpload:
                    GroupFileUploadedEvent?.Invoke(ApiClient, contentObject.ToObject<GroupFileNotice>());
                    break;
                case Notice.GroupAdmin:
                    ProcessGroupAdminNotice(contentObject);
                    break;
                case Notice.GroupDecrease:
                    ProcessGroupMemberDecrease(contentObject);
                    break;
                case Notice.GroupIncrease:
                    ProcessGroupMemberIncrease(contentObject);
                    break;
                case Notice.FriendAdd:
                    FriendAddedEvent?.Invoke(ApiClient, contentObject.ToObject<FriendAddNotice>());
                    break;
                case Notice.GroupBan:
                    // TODO: 此处代码未测试。
                    GroupBanEvent?.Invoke(ApiClient, contentObject.ToObject<GroupBanNotice>());
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

        /// <summary>
        /// 群文件上传。
        /// </summary>
        public event Action<HttpApiClient, GroupFileNotice> GroupFileUploadedEvent;

        /// <summary>
        /// 已设置新的群管理员。
        /// </summary>
        public event Action<HttpApiClient, GroupAdminNotice> GroupAdminSetEvent;

        /// <summary>
        /// 已取消群管理员。
        /// </summary>
        public event Action<HttpApiClient, GroupAdminNotice> GroupAdminUnsetEvent;

        /// <summary>
        /// 好友添加。
        /// </summary>
        public event Action<HttpApiClient, FriendAddNotice> FriendAddedEvent;

        /// <summary>
        /// 群成员减少。
        /// </summary>
        public event Action<HttpApiClient, GroupMemberDecreaseNotice> GroupMemberDecreasedEvent;

        /// <summary>
        /// 被踢出群。
        /// </summary>
        public event Action<HttpApiClient, KickedNotice> KickedEvent;

        /// <summary>
        /// 群成员增加。
        /// </summary>
        public event Action<HttpApiClient, GroupMemberIncreaseNotice> GroupMemberIncreasedEvent;

        /// <summary>
        /// 加入新群时发生的事件。注意此事件没有 <see cref="GroupMemberChangeNotice.OperatorId"/> 的数据（至少 Invite 没有，Approve 不清楚）。
        /// </summary>
        public event Action<HttpApiClient, GroupMemberIncreaseNotice> GroupAddedEvent;

        /// <summary>
        /// 群禁言事件。包括禁言和解除禁言。
        /// </summary>
        public event Action<HttpApiClient, GroupBanNotice> GroupBanEvent;

        #endregion

        #region Request

        /// <summary>
        /// 收到加群请求事件。
        /// </summary>
        public event GroupRequestEventHandler GroupRequestEvent;

        private RequestResponse GroupRequestHappen(GroupRequest request)
            => GetFirstResponseOrDefault(GroupRequestEvent, h => h.Invoke(ApiClient, request));

        /// <summary>
        /// 收到加群邀请事件。此时 <see cref="Request.Comment"/> 并不存在。
        /// </summary>
        public event GroupRequestEventHandler GroupInviteEvent;

        private RequestResponse GroupInviteHappen(GroupRequest request)
            => GetFirstResponseOrDefault(GroupInviteEvent, h => h.Invoke(ApiClient, request));

        /// <summary>
        /// 收到好友请求事件。
        /// </summary>
        public event FriendRequestEventHandler FriendRequestEvent;

        private RequestResponse FriendRequestHappen(FriendRequest request)
            => GetFirstResponseOrDefault(FriendRequestEvent, h => h.Invoke(ApiClient, request));

        private static TResponse GetFirstResponseOrDefault<TResponse, THandler>(THandler handler, Func<THandler, TResponse> invoker)
            where THandler : Delegate
            where TResponse : class
            => handler?.GetInvocationList().Cast<THandler>().Select(invoker)
                .FirstOrDefault(response => response != null);

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
        {
            try
            {
                await api.SendMessageAsync(message.Endpoint, message.Content).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 当收到消息时，在同一处发送指定内容消息的事件处理器。并没有什么卵用。
        /// </summary>
        /// <param name="something"></param>
        /// <returns></returns>
        public static MessageEventHandler Say(string something)
            => (api, message) => api?.SendMessageAsync(message.Endpoint, something);

        //public static void Print(HttpApiClient api, GroupMessage notice)
        //    => Console.WriteLine(notice.Content);

        #endregion
    }
}
