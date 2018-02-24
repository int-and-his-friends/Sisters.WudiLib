using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sisters.WudiLib
{
    /// <summary>
    /// 表示消息。
    /// </summary>
    public class Message
    {
        private readonly ICollection<Section> sections = new LinkedList<Section>();

        internal ICollection<Section> Sections { get; } = new LinkedList<Section>();

        /// <summary>
        /// 构造新的消息实例
        /// </summary>
        public Message() { }

        /// <summary>
        /// 从文本构造新的消息实例
        /// </summary>
        /// <param name="text"></param>
        public Message(string text) => Sections.Add(Section.Text(text));

        private Message(Message message1, Message message2)
        {
            sections = new LinkedList<Section>(message1.Sections.Union(message2.Sections));
        }

        public static Message operator +(Message left, Message right) => new Message(left, right);

        internal class Section// : IEquatable<Section>
        {
            [JsonProperty("type")]
            private readonly string type;

            [JsonIgnore]
            internal string Type => type;

            [JsonProperty("data")]
            private readonly Dictionary<string, string> data = new Dictionary<string, string>();

            [JsonIgnore]
            internal IReadOnlyDictionary<string, string> Data => data;

            private Section(string type) => this.type = type;

            internal static Section Text(string text)
            {
                var section = new Section("text");
                section.data.Add("text", text);
                return section;
            }

            //public override bool Equals(object obj) => this.Equals(obj as Section);
            //public bool Equals(Section other) => other != null && this.type == other.type && EqualityComparer<Dictionary<string, string>>.Default.Equals(this.data, other.data);

            //public override int GetHashCode()
            //{
            //    var hashCode = -628614918;
            //    hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.type);
            //    hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<string, string>>.Default.GetHashCode(this.data);
            //    return hashCode;
            //}

            //public static bool operator ==(Section left, Section right)
            //{
            //    if (left.Type != right.Type) return false;
            //    if (left.Data.Count != right.Data.Count) return false;
            //    foreach (var item in left.Data)
            //    {
            //        string key = item.Key;
            //        if (right.Data.TryGetValue(key, out string rightValue))
            //            if (item.Value == rightValue) continue;
            //        return false;
            //    }
            //    return true;
            //}

            //public static bool operator !=(Section left, Section right) => !(left == right);
        }
    }
}