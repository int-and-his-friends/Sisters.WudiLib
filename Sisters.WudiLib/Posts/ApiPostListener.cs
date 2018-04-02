using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Sisters.WudiLib.Posts
{
    public class ApiPostListener
    {
        #region
        public HttpApiClient ApiClient { get; set; }

        /// <summary>
        /// 获取或设置 HTTP API 的上报地址。如果已经开始监听，则设置无效。
        /// </summary>
        public string PostAddress { get; set; }

        private readonly object listenerLock = new object();

        private HttpListener listener = new HttpListener();

        private Task listenTask;

        /// <summary>
        /// 获取当前是否监听 HTTP API 的上报数据。如果状态设置失败，此属性不会改变。
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
                var request = context.Request;
                object responseObject;
                string requestContent;

                using (var inStream = request.InputStream)
                using (var streamReader = new StreamReader(inStream))
                    requestContent = streamReader.ReadToEnd();

                using (var response = context.Response)
                {
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
                }
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
                case Post.MessageType:
                    ProcessMessage(content, post);
                    return null;
                case Post.EventType:
                    return null;
                case Post.RequestType:
                    return ProcessRequest(content);
            }
            return null;
        }

        private void ProcessMessage(string content, GroupMessage groupMessage)
        {
            switch (groupMessage.MessageType)
            {
                case Message.PrivateType:

                default:
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

        #region GroupRequest
        private readonly ICollection<GroupRequestEventHandler> groupRequestEventHandlers = new LinkedList<GroupRequestEventHandler>();

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

        #region DefaultHandlers
        public static GroupRequestResponse ApproveAllGroupRequests(HttpApiClient api, GroupRequest groupRequest)
            => new GroupRequestResponse { Approve = true };

        public static FriendRequestResponse ApproveAllFriendRequests(HttpApiClient api, FriendRequest friendRequest)
            => new FriendRequestResponse { Approve = true };
        #endregion
    }
}
