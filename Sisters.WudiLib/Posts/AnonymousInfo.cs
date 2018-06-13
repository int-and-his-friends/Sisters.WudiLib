using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sisters.WudiLib.Posts
{
    public sealed class AnonymousInfo
    {
        [JsonProperty("flag")]
        public string Flag { get; private set; }
        [JsonProperty("id")]
        public int Id { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }
    }
}
