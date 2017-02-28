using System;
using System.Collections.Generic;
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

            // Act
            var response = await webRequester.GetResponseAsync("");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task GetResponseAsync_ShouldTimeoutOnSlowResponse()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).WithTimeout(TimeSpan.FromMilliseconds(100)).Create();
            var webRequester = new WebRequester(httpClient);

            // Act
            var exception = await Record.ExceptionAsync(async () => await webRequester.GetResponseAsync("/delay/250"));

            // Assert
            exception.ShouldBeOfType(typeof(TaskCanceledException));
        }

        [Fact]
        public async Task GetResponseAsync_ShouldSetDefaultHeaders()
        {
            // Arrange
            var httpClient =
                WebRequestHttpClientFactory.Configure(new Uri(Url))
                    .WithDefaultRequestHeaders(new Dictionary<string, string> { { "fakeHeader", "fakeValue" } })
                    .Create();
            var webRequester = new WebRequester(httpClient);

            // Act
            var response = await webRequester.GetResponseAsStringAsync("/headers");

            // Assert
            response.Contains("fakeHeader").ShouldBeTrue();
        }

        public void Dispose()
        {
            _webApp.Dispose();
        }
    }
}