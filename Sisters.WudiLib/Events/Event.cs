using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Sisters.WudiLib.Events
{
    class Event
    {
        [JsonProperty("post_type")]
        public string PostType { get; set; }
    }
}
