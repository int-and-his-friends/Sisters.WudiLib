using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib.Posts;

namespace Sisters.WudiLib.WebSocket
{
    internal static class WebSocketEventExtensions
    {
        internal static async Task ProcessWSMessageAsync(this ApiPostListener listener, JObject eventObject)
        {
            await Task.Yield();
            var response = listener.ProcessPost(eventObject);
            var apiClient = listener.ApiClient;
            if (response is RequestResponse && !(apiClient is null))
            {
                JObject data = eventObject;
                data.Merge(JObject.FromObject(response));
                switch (response)
                {
                    case FriendRequestResponse friend:
                        await apiClient.HandleFriendRequestInternalAsync(data).ConfigureAwait(false);
                        break;
                    case GroupRequestResponse group:
                        await apiClient.HandleGroupRequestInternalAsync(data).ConfigureAwait(false);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
