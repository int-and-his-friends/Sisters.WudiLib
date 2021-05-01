using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.WebSocket
{
    public interface IRequestSender
    {
        Action<byte[], JObject> OnResponse { get; set; }
        /// <summary>
        /// Used for cleanup.
        /// </summary>
        Action OnSocketDisconnected { get; set; }

        Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default);
    }
}
