using System;
using Newtonsoft.Json.Linq;

namespace Sisters.WudiLib.WebSocket
{
    public interface IEventReceiver
    {
        Action<byte[], JObject> OnEvent { get; set; }
    }
}
