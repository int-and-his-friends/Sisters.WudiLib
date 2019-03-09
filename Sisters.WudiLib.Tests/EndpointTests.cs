using System;
using System.Collections.Generic;
using System.Text;
using Sisters.WudiLib.Posts;
using Xunit;

namespace Sisters.WudiLib.Tests
{
    public class EndpointTests
    {
        [Fact]
        public void Endpoint_ToString()
        {
            var endpoints = new Endpoint[] { new PrivateEndpoint(111), new GroupEndpoint(222), new DiscussEndpoint(333) };
            var strings = new[] { "private/111", "group/222", "discuss/333" };

            for (int i = 0; i < endpoints.Length; i++)
            {
                Assert.Equal(strings[i], endpoints[i].ToString());
            }
        }
    }
}
