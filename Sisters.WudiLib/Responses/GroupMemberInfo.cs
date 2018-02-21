using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sisters.WudiLib.Responses
{
    public sealed class GroupMemberInfo
    {
        [JsonProperty("group_id")]
        public long GroupId { get; set; }
        [JsonProperty("user_id")]
        public long UserId { get; set; }
        [JsonProperty("nickname")]
        public string QqNickname { get; set; }
        [JsonProperty("card")]
        public string InGroupName { get; set; }

        [JsonProperty("role"), JsonConverter(typeof(AuthorityConverter))]
        public GroupMemberAuthority Authority { get; set; }
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

        private class AuthorityConverter : JsonConverter
        {
            private static readonly IReadOnlyDictionary<string, GroupMemberAuthority> list = new Dictionary<string, GroupMemberAuthority>
            {
                { "member", GroupMemberAuthority.Normal },
                { "admin", GroupMemberAuthority.Manager },
                { "owner", GroupMemberAuthority.Leader },
            };

            public override bool CanConvert(Type objectType) => objectType == typeof(GroupMemberAuthority);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType != JsonToken.String)
                    return GroupMemberAuthority.Unknown;
                return list.GetValueOrDefault(reader.Value.ToString(), GroupMemberAuthority.Unknown);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        }

        private class EnumConverter<T> : JsonConverter where T : IDictionaryProvider
        {
            public override bool CanConvert(Type objectType) => throw new NotImplementedException();
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                typeof(IDictionaryProvider).GetCustomAttributes<>
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        }

        private interface IDictionaryProvider
        {
            object Default { get; }

            IReadOnlyDictionary<string, object> Values { get; }


        }
    }
}
