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
    }
}
