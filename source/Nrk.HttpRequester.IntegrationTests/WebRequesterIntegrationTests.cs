using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nrk.HttpRequester.IntegrationTests.TestServer;
using Shouldly;
using Xunit;

namespace Nrk.HttpRequester.IntegrationTests
{
    public class WebRequesterIntegrationTests : IDisposable
    {
        private readonly IDisposable _webApp;
        private readonly IWebRequester _webRequester;
        private const string Url = "http://localhost:9001";

        public WebRequesterIntegrationTests()
        {
            _webApp = WebApp.Start<Startup>(Url);
            _webRequester = new WebRequester(WebRequestHttpClientFactory.Configure(new Uri(Url)).Create());
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
        public async Task GetResponseAsync_ShouldGetResponseFromServer()
        {
            // Act
            var response = await _webRequester.GetResponseAsync("/");

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task GetResponseAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            const string authorizationScheme = "bearer";
            const string accessToken = "accessToken";
            // Act
            var response = await _webRequester.GetResponseAsync("/headers", new AuthenticationHeaderValue(authorizationScheme, accessToken));

            // Assert
            VerifyAuthorizationHeader(response, authorizationScheme, accessToken);
        }

        [Fact]
        public async Task GetResponseAsStringAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            const string authorizationScheme = "bearer";
            const string accessToken = "accessToken";
            // Act
            var response = await _webRequester.GetResponseAsStringAsync("/headers", new AuthenticationHeaderValue(authorizationScheme, accessToken));

            // Assert
            response.ShouldContain("Authorization");
            response.ShouldContain("bearer accessToken");
        }

        [Fact]
        public async Task PutAsync_ShouldGetResponseFromServer()
        {
            // Act
            var response = await _webRequester.PutAsync("/", new StringContent("test"));

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task PutAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var content = new StringContent("");
            const string authorizationScheme = "bearer";
            const string accessToken = "accessToken";
            // Act
            var response = await _webRequester.PutAsync("/headers", content, new AuthenticationHeaderValue(authorizationScheme, accessToken));

            // Assert
            VerifyAuthorizationHeader(response, authorizationScheme, accessToken);
        }

        [Fact]
        public async Task PutAsync_ShouldSetContent()
        {
            // Arrange
            const string exampleContent = "Sample content";
            // Act
            var response = await _webRequester.PutAsync("/content", new StringContent(exampleContent));

            // Assert
            var actualContent = await response.Content.ReadAsStringAsync();
            actualContent.ShouldBe(exampleContent);
        }

        [Fact]
        public async Task PostAsync_ShouldGetResponseFromServer()
        {
            // Act
            var response = await _webRequester.PostAsync("/", new StringContent("test"));

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task PostAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var content = new StringContent("");
            const string authorizationScheme = "bearer";
            const string accessToken = "accessToken";
            // Act
            var response = await _webRequester.PostAsync("/headers", content, new AuthenticationHeaderValue(authorizationScheme, accessToken));

            // Assert
            VerifyAuthorizationHeader(response, authorizationScheme, accessToken);
        }

        [Fact]
        public async Task PostAsync_ShouldSetContent()
        {
            // Arrange
            const string exampleContent = "Sample content";
            // Act
            var response = await _webRequester.PostAsync("/content", new StringContent(exampleContent));

            // Assert
            var actualContent = await response.Content.ReadAsStringAsync();
            actualContent.ShouldBe(exampleContent);
        }

        [Fact]
        public async Task DeleteAsync_ShouldGetResponseFromServer()
        {
            // Act
            var response = await _webRequester.DeleteAsync("/", new StringContent("test"));

            // Assert
            response.IsSuccessStatusCode.ShouldBeTrue();
        }

        [Fact]
        public async Task DeleteAsync_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var content = new StringContent("");
            const string authorizationScheme = "bearer";
            const string accessToken = "accessToken";

            // Act
            var response = await _webRequester.DeleteAsync("/headers", content, new AuthenticationHeaderValue(authorizationScheme, accessToken));

            // Assert
            VerifyAuthorizationHeader(response, authorizationScheme, accessToken);
        }

        [Fact]
        public async Task DeleteAsync_ShouldSetContent()
        {
            // Arrange
            const string exampleContent = "Sample content";

            // Act
            var response = await _webRequester.DeleteAsync("/content", new StringContent(exampleContent));

            // Assert
            var actualContent = await response.Content.ReadAsStringAsync();
            actualContent.ShouldBe(exampleContent);
        }

        private static void VerifyAuthorizationHeader(HttpResponseMessage response, string authorizationScheme, string accessToken)
        {
            response.RequestMessage.Headers.Authorization.Scheme.ShouldBe(authorizationScheme);
            response.RequestMessage.Headers.Authorization.Parameter.ShouldBe(accessToken);
        }

        public void Dispose()
        {
            _webApp.Dispose();
        }
    }
}
