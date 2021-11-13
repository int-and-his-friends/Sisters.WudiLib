using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Sisters.WudiLib.Tests
{
    public class SendingMessageTests
    {
        [Fact]
        public void Ctor()
        {
            string message = "123[";
            string raw = "123&#91;";
            var msg1 = new SendingMessage(message);
            Assert.Equal(raw, msg1.Raw);
            //Assert.Equal(message, msg1.ToString());

            var atSection = new Section("at", ("qq", "all"));
            var atMsg = SendingMessage.AtAll();
            Section atMsgSingleSection = atMsg.Sections.Single();
            Assert.Equal(atSection, atMsgSingleSection);
        }

        [Theory]
        [InlineData("a.jpg", "a.jpg")]
        [InlineData("/a b.jpg", "file:///a%20b.jpg")]
        [InlineData(@"c:\a.jpg", "file:///c:/a.jpg")]
        [InlineData("/dir name%20/file?ww=33", "file:///dir%20name%2520/file%3Fww=33")]
        public void LocalImageTests(string path, string expected)
        {
            var image = SendingMessage.LocalImage(path);
            Assert.Equal(new KeyValuePair<string, string>("file", expected), image.Sections.Single().Data.Single());
        }

        [Fact]
        public void Concat()
        {
            var msg1 = new SendingMessage("1");
            var msg2 = msg1 + "2";
            var msg3 = "3" + msg2;
            Assert.IsType<SendingMessage>(msg2);
            Assert.IsType<SendingMessage>(msg3);
        }

        [Fact]
        public void ConcatNull()
        {
            SendingMessage message1 = null, message2 = null;
            Assert.NotNull(message1 + message2);
            Assert.False((message1 + message2)?.Sections.Count > 0);

            SendingMessage text = "hello";
            Assert.Equal(text.Sections, (message1 + text).Sections);
            Assert.Equal(text.Sections, (text + message1).Sections);
        }

        public static IEnumerable<object[]> Interpolated_TestData()
        {
            var cqCodeMessage = SendingMessage.At(123456789);
            var textMessage = "hello";
            var imageMessage = SendingMessage.LocalImage("/a.jpg");
            var multipleSegmentMessage = SendingMessage.At(123456789) + "hello" + SendingMessage.LocalImage("/a.jpg");
            {
                yield return new object[] { SendingMessage.FromInterpolated($"{1}"), "1" };
                yield return new object[] { SendingMessage.FromInterpolated($"{cqCodeMessage}，你好"), cqCodeMessage + "，你好" };
                yield return new object[] { SendingMessage.FromInterpolated($"{multipleSegmentMessage}，你好"), multipleSegmentMessage + "，你好" };
                yield return new object[] { SendingMessage.FromInterpolated($"{cqCodeMessage}{textMessage}"), cqCodeMessage + textMessage };
                yield return new object[] { SendingMessage.FromInterpolated($"{textMessage}{cqCodeMessage},[]& {23}"), textMessage + cqCodeMessage + ",[]& " + 23.ToString() };
            }
        }

        [Theory]
        [MemberData(nameof(Interpolated_TestData))]
        public void Interpolated(SendingMessage expected, SendingMessage actual)
        {
            Assert.Equal(expected.Raw, actual.Raw);
            Assert.Equal(expected.Sections.Count, actual.Sections.Count);
            for (int i = 0; i < expected.Sections.Count; i++)
            {
                Assert.Equal(expected.Sections[i], actual.Sections[i]);
            }
        }
    }
}
