using System;
using System.Collections.Generic;
using System.Net.Http;
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

        [Fact]
        public async Task PutAsync_ShouldGetResponseFromServer()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            // Act
            var response = await webRequester.PutAsync("", "", "", new StringContent("test"));

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task PutAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            var content = new StringContent("");
            const string authorizationScheme = "bearer";
            const string accessToken = "accessToken";
            // Act
            var response = await webRequester.PutAsync("/headers", authorizationScheme, accessToken, content);

            // Assert
            response.RequestMessage.Headers.Authorization.Scheme.ShouldBe(authorizationScheme);
            response.RequestMessage.Headers.Authorization.Parameter.ShouldBe(accessToken);
        }

        [Fact]
        public async Task PutAsync_ShouldSetContent()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            const string exampleContent = "Sample content";
            // Act
            var response = await webRequester.PutAsync("/content", "", "", new StringContent(exampleContent));

            // Assert
            var actualContent = await response.Content.ReadAsStringAsync();
            actualContent.ShouldBe(exampleContent);
        }

        [Fact]
        public async Task PostAsync_ShouldGetResponseFromServer()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            // Act
            var response = await webRequester.PostAsync("", "", "", new StringContent("test"));

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task PostAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            var content = new StringContent("");
            const string authorizationScheme = "bearer";
            const string accessToken = "accessToken";
            // Act
            var response = await webRequester.PostAsync("/headers", authorizationScheme, accessToken, content);

            // Assert
            response.RequestMessage.Headers.Authorization.Scheme.ShouldBe(authorizationScheme);
            response.RequestMessage.Headers.Authorization.Parameter.ShouldBe(accessToken);
        }

        [Fact]
        public async Task PostAsync_ShouldSetContent()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            const string exampleContent = "Sample content";
            // Act
            var response = await webRequester.PostAsync("/content", "", "", new StringContent(exampleContent));

            // Assert
            var actualContent = await response.Content.ReadAsStringAsync();
            actualContent.ShouldBe(exampleContent);
        }

        [Fact]
        public async Task DeleteAsync_ShouldGetResponseFromServer()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            // Act
            var response = await webRequester.DeleteAsync("", "", "", new StringContent("test"));

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            var content = new StringContent("");
            const string authorizationScheme = "bearer";
            const string accessToken = "accessToken";

            // Act
            var response = await webRequester.DeleteAsync("/headers", authorizationScheme, accessToken, content);

            // Assert
            response.RequestMessage.Headers.Authorization.Scheme.ShouldBe(authorizationScheme);
            response.RequestMessage.Headers.Authorization.Parameter.ShouldBe(accessToken);
        }

        [Fact]
        public async Task DeleteAsync_ShouldSetContent()
        {
            // Arrange
            var httpClient = WebRequestHttpClientFactory.Configure(new Uri(Url)).Create();
            var webRequester = new WebRequester(httpClient);

            const string exampleContent = "Sample content";
            // Act
            var response = await webRequester.DeleteAsync("/content", "", "", new StringContent(exampleContent));

            // Assert
            var actualContent = await response.Content.ReadAsStringAsync();
            actualContent.ShouldBe(exampleContent);
        }

        public void Dispose()
        {
            _webApp.Dispose();
        }
    }
}
