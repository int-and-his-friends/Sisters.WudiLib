using System;
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

            var section = new Section(Section.MusicType, ("type", "cus"), ("source", "test"));
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
        }

        [Fact]
        public void Constructor_ArgNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Section("text", ((string, string)[])null));
        }
    }
}
