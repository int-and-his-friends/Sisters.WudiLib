using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Sisters.WudiLib.Tests
{
    public class SectionTests
    {
        [Fact]
        public void CreateAndEqualTest()
        {
            var atAll = SendingMessage.AtAll();
            Assert.Equal("[CQ:at,qq=all]", atAll.Raw);

            //var section = new Section(Section.MusicType, ("type", "cus"), ("source", "test"));
            var section = new Section(Section.MusicType, new Dictionary<string, string>
            {
                { "type", "cus" },
                { "source", "test" },
            });
            var section2 = new Section(Section.MusicType, ("source", "test"), ("type", "cus"));
            var json = JsonConvert.SerializeObject(section);
            var jObj = JsonConvert.DeserializeObject<JObject>(json);
            Assert.Equal(Section.MusicType, jObj["type"].ToObject<string>());
            var desSection = jObj.ToObject<Section>();
            Assert.Equal(section, desSection);
            Assert.Equal(section, section2);
            Assert.Equal(desSection, section2);
            Assert.Equal(section.GetHashCode(), desSection.GetHashCode());
            Assert.Equal(section.GetHashCode(), section2.GetHashCode());
            Assert.Equal(desSection.GetHashCode(), section2.GetHashCode());
            Assert.NotEqual(section.Raw, section2.Raw);
        }

        [Fact]
        public void Equal_DataOrder()
        {
            var dic1 = new Dictionary<string, string>
            {
                { "para1", "arg1" },
                { "para2", "arg2" },
            };
            var dic2 = new Dictionary<string, string>
            {
                { "para2", "arg2" },
                { "para1", "arg1" },
            };
            var section1 = new Section("type", dic1);
            var section2 = new Section("type", dic2);

            Assert.Equal(section1, section2);
        }

        [Fact]
        public void Constructor_ArgNull()
        {
            Assert.Throws<ArgumentNullException>("data", () => new Section("text", ((string, string)[])null));
            Assert.Throws<ArgumentNullException>("data", () => new Section("text", (IReadOnlyDictionary<string, string>)null));
            Assert.Throws<ArgumentNullException>("type", () => new Section(null, ("p", "a")));
            Assert.Throws<ArgumentNullException>("type", () => new Section(null, new Dictionary<string, string>()));
        }

        [Fact]
        public void Constructor_ArgIllegal()
        {
            new Section("m.0_-", ("key", "value"));
            Assert.Throws<ArgumentException>("type", () => new Section("m.0_- "));
            Assert.Throws<ArgumentException>("type", () => new Section(""));
            Assert.Throws<ArgumentException>("type", () => new Section(" "));
            Assert.Throws<ArgumentException>("data", () => new Section("sss", ("!", "123")));
            Assert.Throws<ArgumentException>("data", () => new Section("sss", ("", "233")));
            Assert.Throws<ArgumentException>("data", () => new Section("sss", (" ", "233")));
        }

        [Fact]
        public void Data_CheckReadOnly()
        {
            var data = new NotReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                ["para"] = "arg",
            });
            var section = new Section("some_type", data);

            var count1 = section.Data.Count;
            data.Add("keykey", "vvv");
            var count2 = section.Data.Count;

            Assert.Equal(1, count1);
            Assert.Equal(count1, count2);

            var dictionary = new Dictionary<string, string>
            {
                ["para"] = "arg",
            };
            var data2 = new ReadOnlyDictionary<string, string>(dictionary);
            var section2 = new Section("some_type", data2);
            dictionary.Add("diff_key", "val");
            var count = section2.Data.Count;
            Assert.Equal(1, count);
        }

        class NotReadOnlyDictionary<TKey, TValue> : ReadOnlyDictionary<TKey, TValue>
        {
            public NotReadOnlyDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
            {
            }

            public void Add(TKey key, TValue value)
            {
                Dictionary.Add(key, value);
            }
        }
    }
}
