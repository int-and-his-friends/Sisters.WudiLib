using System.Collections.Generic;
using Sisters.WudiLib.Posts;
using Xunit;

namespace Sisters.WudiLib.Tests
{
    public class ListenerTests
    {
        /// <summary>
        /// 测试文本上报类型中，获取 <see cref="Section"/> 列表时能否正确地处理转义字符。
        /// </summary>
        [Fact]
        public void Listener_TextPostTypeSectionsUnescape()
        {
            string json = @"{""anonymous"":null,""font"":336542616,""group_id"":514661057,""message"":""绑定[CQ:at,qq=962549599] &#91; Morion &#93;:测试"",""message_id"":745339,""message_type"":""group"",""post_type"":""message"",""raw_message"":""绑定[CQ:at,qq=962549599] &#91; Morion &#93;:测试"",""self_id"":122866607,""sender"":{""age"":21,""card"":""钻石 | 动漫站建不成了"",""nickname"":""ymy😂/pch"",""sex"":""male"",""user_id"":962549599},""sub_type"":""normal"",""time"":1541558577,""user_id"":962549599}";
            var listener = new ApiPostListener();
            IReadOnlyList<Section> sections = null;
            listener.MessageEvent += (api, e) =>
            {
                var content = e.Content;
                sections = content.Sections;
            };
            listener.ProcessPost(json, null);

            // 
            Assert.NotNull(sections);
            Assert.Equal(3, sections.Count);

            // Section 1

            // Section 2
            Assert.Equal<KeyValuePair<string, string>>(new SortedDictionary<string, string>
            {
                ["qq"] = "962549599"
            }, sections[1].Data);
            Assert.Equal("at", sections[1].Type);

            // Section 3
            // 应该正确转义 " &#91; Morion &#93;:测试" 为下面的内容。
            Assert.Equal(" [ Morion ]:测试", sections[2].ToString());
        }
    }
}
