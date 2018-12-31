using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS1591

namespace Sisters.WudiLib.Responses
{
    [JsonObject(MemberSerialization.OptIn)]
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

        public string DisplayName => string.IsNullOrEmpty(InGroupName) ? Nickname : InGroupName;

        /// <summary>
        /// 性别。
        /// </summary>
        [JsonProperty("sex"), JsonConverter(typeof(StringEnumConverter))]
        public Sex Sex { get; private set; }

        [JsonProperty("age")]
        public int Age { get; internal set; }

        /// <summary>
        /// 地区。<see cref="HttpApiClient.GetGroupMemberListAsync(long)"/> 中无法获取。
        /// </summary>
        [JsonProperty("area")]
        public string Area { get; internal set; }

        [JsonProperty("join_time"), JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset JoinTime { get; internal set; }

        [JsonProperty("last_sent_time"), JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset LastSendTime { get; internal set; }

        /// <summary>
        /// 成员等级。<see cref="HttpApiClient.GetGroupMemberListAsync(long)"/> 中无法获取。
        /// </summary>
        [JsonProperty("level")]
        public string Level { get; private set; }

        [JsonProperty("role"), JsonConverter(typeof(AuthorityConverter))]
        public GroupMemberAuthority Authority { get; internal set; }

        /// <summary>
        /// 是否不良记录成员。
        /// </summary>
        [JsonProperty("unfriendly")]
        public bool IsUnfriendly { get; private set; }

        /// <summary>
        /// 专属头衔。<see cref="HttpApiClient.GetGroupMemberListAsync(long)"/> 中无法获取。
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; internal set; }

        // title_expire_time

        [JsonProperty("card_changeable")]
        public bool IsCardChangeable { get; internal set; }

        public override string ToString() => DisplayName;

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
    }
}

#pragma warning restore CS1591