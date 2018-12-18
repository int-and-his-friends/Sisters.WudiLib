using System;
using System.Collections.Generic;
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
        }
    }
}
