using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sisters.WudiLib.Responses
{
    public sealed class GroupMemberInfo
    {
        [JsonProperty("group_id")]
        public long GroupId { get; internal set; }
        [JsonProperty("user_id")]
        public long UserId { get; internal set; }
        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }
        [JsonProperty("card")]
        public string InGroupName { get; internal set; }

        // sex

        [JsonProperty("age")]
        public int Age { get; internal set; }

        [JsonProperty("area")]
        public string Area { get; internal set; }

        [JsonProperty("join_time"), JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset JoinTime { get; internal set; }

        [JsonProperty("last_sent_time"), JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset LastSendTime { get; internal set; }

        // level （？）

        [JsonProperty("role"), JsonConverter(typeof(AuthorityConverter))]
        public GroupMemberAuthority Authority { get; internal set; }

        // unfriendly

        [JsonProperty("title")]
        public string Title { get; internal set; }

        // title_expire_time

        [JsonProperty("card_changeable")]
        public bool IsCardChangeable { get; internal set; }

        /*
        public int age { get; set; }
        public string area { get; set; }
        public string card { get; set; }
        public bool card_changeable { get; set; }
        public long group_id { get; set; }
        public int join_time { get; set; }
        public int last_sent_time { get; set; }
        public string level { get; set; }
        public string nickname { get; set; }
        public string role { get; set; }
        public string sex { get; set; }
        public string title { get; set; }
        public int title_expire_time { get; set; }
        public bool unfriendly { get; set; }
        public long user_id { get; set; }
         */
        public enum GroupMemberAuthority
        {
            Unknown = 0,
            Normal = 1,
            Manager = 2,
            Leader = 3,
        }

        internal class AuthorityConverter : JsonConverter
        {
            private static readonly IReadOnlyDictionary<string, GroupMemberAuthority> List =
                new Dictionary<string, GroupMemberAuthority>
                {
                    { "member", GroupMemberAuthority.Normal },
                    { "admin", GroupMemberAuthority.Manager },
                    { "owner", GroupMemberAuthority.Leader },
                };

            public override bool CanConvert(Type objectType) => objectType == typeof(GroupMemberAuthority);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                return reader.TokenType == JsonToken.String
                    ? List.GetValueOrDefault(reader.Value.ToString(), GroupMemberAuthority.Unknown)
                    : GroupMemberAuthority.Unknown;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var result = from e in List
                             where e.Value == value as GroupMemberAuthority?
                             select e.Key;
                if (!result.Any()) writer.WriteNull();
                else writer.WriteValue(result.First());
            }
        }

        //private class EnumConverter<T> : JsonConverter where T : IDictionaryProvider
        //{
        //    public override bool CanConvert(Type objectType) => throw new NotImplementedException();
        //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //    {
        //        if (reader.TokenType != JsonToken.String)
        //            return T.Default;
        //    }

        //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        //}

        //private interface IDictionaryProvider
        //{
        //    object Default { get; }

        //    IReadOnlyDictionary<string, object> Values { get; }


        //}
    }
}
