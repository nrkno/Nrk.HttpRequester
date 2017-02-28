using System;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Nrk.HttpRequester.IntegrationTests.TestServer;
using Shouldly;
using Xunit;

namespace Nrk.HttpRequester.IntegrationTests
{
    public class WebRequesterIntegrationTests : IDisposable
    {
        private readonly IDisposable _webApp;
        private const string Url = "http://localhost:9001";
        public WebRequesterIntegrationTests()
        {
            _webApp = WebApp.Start<Startup>(Url);
        }

        [Fact]
        public async Task GetResponseAsync_ShouldGetResponseFromServer()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);
            var response = await webRequester.GetResponseAsync("");
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        public void Dispose()
        {
            _webApp.Dispose();
        }
    }
}