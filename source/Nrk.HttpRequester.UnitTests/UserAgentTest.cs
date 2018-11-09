using System.Net.Http;
using Nrk.HttpRequester.UnitTests.TestData;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Nrk.HttpRequester.UnitTests
{
    public class UserAgentTest
    {
        private readonly ITestOutputHelper _output;

        public UserAgentTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Product_Version()
        {
            var userAgent = new UserAgent(Some.Product, Some.Version);
            var header = GetUserAgentHeader(userAgent);
            _output.WriteLine(header);
            header.ShouldBe($"{Some.Product}/{Some.Version}");
        }

        [Fact]
        public void Product_Version_Comment()
        {
            var userAgent = new UserAgent(Some.Product, Some.Version, Some.Comment);
            var header = GetUserAgentHeader(userAgent);
            _output.WriteLine(header);
            header.ShouldBe($"{Some.Product}/{Some.Version} ({Some.Comment})");
        }

        [Fact]
        public void Product_Version_DataCenter()
        {
            var userAgent = new UserAgent(Some.Product, Some.Version).Add(Some.DataCenter);
            var header = GetUserAgentHeader(userAgent);
            _output.WriteLine(header);
            header.ShouldBe($"{Some.Product}/{Some.Version} {Some.DataCenter}");
        }

        [Fact]
        public void Product_Version_DataCenter_MachineName()
        {
            var userAgent = new UserAgent(Some.Product, Some.Version).Add(Some.DataCenter).Add(Some.MachineName);
            var header = GetUserAgentHeader(userAgent);
            _output.WriteLine(header);
            header.ShouldBe($"{Some.Product}/{Some.Version} {Some.DataCenter} {Some.MachineName}");
        }

        [Fact]
        public void Product_Version_Comment_Framework_Version_Comment()
        {
            var userAgent = new UserAgent(Some.Product, Some.Version, Some.Comment).Add(Some.Framework, Some.Version, Some.Comment);
            var header = GetUserAgentHeader(userAgent);
            _output.WriteLine(header);
            header.ShouldBe($"{Some.Product}/{Some.Version} ({Some.Comment}) {Some.Framework}/{Some.Version} ({Some.Comment})");
        }

        private static string GetUserAgentHeader(UserAgent userAgent)
        {
            var userAgentHeader = new HttpRequestMessage().Headers.UserAgent;
            foreach (var info in userAgent.ToProductInfoHeaderValues())
            {
                userAgentHeader.Add(info);
            }
            return userAgentHeader.ToString();
        }
    }
}
