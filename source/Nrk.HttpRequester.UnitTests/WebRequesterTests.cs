using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using Shouldly;
using Xunit;

namespace Nrk.HttpRequester.UnitTests
{
    public class WebRequesterTests
    {
        private readonly HttpResponseMessage _basicResponse;
        private readonly HttpClient _basicClient;

        public WebRequesterTests()
        {
            var baseAdress = new Uri("http://baseadress.com");
            var client = new HttpClient { BaseAddress = baseAdress };

            _basicClient = client;
            _basicResponse = new HttpResponseMessage
            {
                Content = new StringContent("All good")
            };
        }

        [Fact]
        public async Task GetResponseAsync_WithRetries_ShouldRetrySendAsync()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            A.CallTo(() => httpClient.SendAsync(A<HttpRequestMessage>.Ignored)).Throws<TaskCanceledException>().NumberOfTimes(1);
            var requester = new WebRequester(httpClient, TimeSpan.FromMilliseconds(1));

            // Act
            await requester.GetResponseAsync("/test", retries: 3);

            // Assert
            A.CallTo(() => httpClient.SendAsync(A<HttpRequestMessage>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
        }

        [Fact]
        public async Task GetResponseAsync_WithRetries_WithMultipleExceptions_ShouldRetrySendAsync()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            A.CallTo(() => httpClient.SendAsync(A<HttpRequestMessage>.Ignored)).Throws<TaskCanceledException>().NumberOfTimes(2);
            A.CallTo(() => httpClient.SendAsync(A<HttpRequestMessage>.Ignored)).Returns(new HttpResponseMessage(HttpStatusCode.BadRequest)).NumberOfTimes(1);
            var requester = new WebRequester(httpClient, TimeSpan.FromMilliseconds(1));

            // Act
            var res = await requester.GetResponseAsync("/test", retries: 3);

            // Assert
            A.CallTo(() => httpClient.SendAsync(A<HttpRequestMessage>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(4));
        }

        [Fact]
        public async Task GetResponseAsync_WithRetries_ShouldOnlyRetryGivenAmountOfTimes()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            A.CallTo(() => httpClient.SendAsync(A<HttpRequestMessage>.Ignored)).Throws<TaskCanceledException>();
            var requester = new WebRequester(httpClient, TimeSpan.FromMilliseconds(1));

            // Act
            var ex = await Record.ExceptionAsync(async () => { await requester.GetResponseAsync("/test", retries: 3); });

            // Assert
            ex.ShouldBeOfType<TaskCanceledException>();
        }

        [Fact]
        public async Task GetResponseAsStringAsync_NullResponse_ShouldReturnEmptyString()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            A.CallTo(() => httpClient.SendAsync(A<HttpRequestMessage>.Ignored)).Returns(Task.FromResult<HttpResponseMessage>(null));
            var requester = new WebRequester(httpClient);

            // Act
            var result = await requester.GetResponseAsStringAsync("/test");

            // Assert
            result.ShouldBe("");
        }

        [Fact]
        public async Task GetResponseAsStringAsync_WithQueryParameter_ShouldIncludeQueryParameterInRequest()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var fakeResponse = new HttpResponseMessage
            {
                Content = new StringContent("All good")
            };
            var url = "test/path?APIKey=value";
            A.CallTo(
                    () =>
                        httpClient.SendAsync(
                            A<HttpRequestMessage>.Ignored))
                .Returns(fakeResponse);
            var requester = new WebRequester(httpClient);

            // Act
            await requester.GetResponseAsStringAsync(url);

            // Assert
            A.CallTo(
                () =>
                    httpClient.SendAsync(A<HttpRequestMessage>.That.Matches(req => req.RequestUri.ToString().Equals("/" + url)))
            ).MustHaveHappened();
        }

        [Fact]
        public async Task GetResponseAsync_NoParameters_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var requester = new WebRequester(httpClient);

            // Act
            var ex = await Record.ExceptionAsync(async () => { await requester.GetResponseAsync("/test", null, null, 0); });

            // Assert
            ex.ShouldBeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task GetResponseAsStringAsync_NoParameters_ShouldThrowArgumentNullException()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var requester = new WebRequester(httpClient);

            // Act
            var ex = await Record.ExceptionAsync(async () => { await requester.GetResponseAsStringAsync("/test", null, null, 0); });

            // Assert
            ex.ShouldBeOfType<ArgumentNullException>();
        }

        [Fact]
        public async Task GetResponseAsStringAsync_WithTemplate_ShouldBuildCorrectUrl()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var template = "test/{item}/{id}";
            var parameters = new NameValueCollection { { "item", "itemName" }, { "id", "123" } };
            var expectedUrl = "/test/itemName/123";
            A.CallTo(
                    () =>
                        httpClient.SendAsync(
                            A<HttpRequestMessage>.Ignored))
                .Returns(_basicResponse);
            A.CallTo(
                    () =>
                        httpClient.Client)
                .Returns(_basicClient);
            var requester = new WebRequester(httpClient);

            // Act
            await requester.GetResponseAsStringAsync(template, parameters);

            // Assert
            A.CallTo(
                () =>
                    httpClient.SendAsync(
                        A<HttpRequestMessage>.That.Matches(req => req.RequestUri.ToString().Equals(expectedUrl)))
            ).MustHaveHappened();
        }

        [Fact]
        public async Task GetResponseAsync_WithTemplate_ShouldBuildCorrectUrl()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var template = "test/{item}/{id}";
            var parameters = new NameValueCollection { { "item", "itemName" }, { "id", "123" } };
            var expectedUrl = "/test/itemName/123";
            A.CallTo(
                    () =>
                        httpClient.SendAsync(
                            A<HttpRequestMessage>.Ignored))
                .Returns(_basicResponse);
            A.CallTo(
                    () =>
                        httpClient.Client)
                .Returns(_basicClient);
            var requester = new WebRequester(httpClient);

            // Act
            await requester.GetResponseAsync(template, parameters);

            // Assert
            A.CallTo(
                () =>
                    httpClient.SendAsync(
                        A<HttpRequestMessage>.That.Matches(req => req.RequestUri.ToString().Equals(expectedUrl)))
            ).MustHaveHappened();
        }

        [Fact]
        public async Task GetResponseAsync_WithTemplateAndDefaultParameters_ShouldBuildCorrectUrl()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var template = "test/{item}/{id}";
            var defaultParams = new NameValueCollection { { "APIKey", "keyvalue" } };
            var parameters = new NameValueCollection { { "item", "itemName" }, { "id", "123" }, { "isEnabled", "true" } };
            var expectedUrl = "/test/itemName/123?isEnabled=true&APIKey=keyvalue";
            A.CallTo(
                    () =>
                        httpClient.SendAsync(
                            A<HttpRequestMessage>.Ignored))
                .Returns(_basicResponse);
            A.CallTo(
                    () =>
                        httpClient.Client)
                .Returns(_basicClient);
            var requester = new WebRequester(httpClient, TimeSpan.FromMilliseconds(1), defaultParams);

            // Act
            await requester.GetResponseAsync(template, parameters);

            // Assert
            A.CallTo(
                () =>
                    httpClient.SendAsync(
                        A<HttpRequestMessage>.That.Matches(req => req.RequestUri.ToString().Equals(expectedUrl)))
            ).MustHaveHappened();
        }

        [Fact]
        public async Task GetResponseAsync_WithUrlAndDefaultParameters_ShouldBuildCorrectUrl()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var url = "test/itemName/123";
            var defaultParams = new NameValueCollection { { "APIKey", "keyvalue" } };
            var expectedUrl = "/test/itemName/123?APIKey=keyvalue";
            A.CallTo(
                    () =>
                        httpClient.SendAsync(
                            A<HttpRequestMessage>.Ignored))
                .Returns(_basicResponse);
            A.CallTo(
                    () =>
                        httpClient.Client)
                .Returns(_basicClient);
            var requester = new WebRequester(httpClient, TimeSpan.FromMilliseconds(1), defaultParams);

            // Act
            await requester.GetResponseAsync(url);

            // Assert
            A.CallTo(
                () =>
                    httpClient.SendAsync(
                        A<HttpRequestMessage>.That.Matches(req => req.RequestUri.ToString().Equals(expectedUrl)))
            ).MustHaveHappened();
        }

        [Fact]
        public async Task Send_WithUrlAndDefaultParameters_ShouldBuildCorrectUrl()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var url = "test/itemName/123";
            var defaultParams = new NameValueCollection { { "APIKey", "keyvalue" } };
            var expectedUrl = "/test/itemName/123?APIKey=keyvalue";
            A.CallTo(
                    () =>
                        httpClient.SendAsync(
                            A<HttpRequestMessage>.Ignored))
                .Returns(_basicResponse);
            A.CallTo(
                    () =>
                        httpClient.Client)
                .Returns(_basicClient);
            var requester = new WebRequester(httpClient, TimeSpan.FromMilliseconds(1), defaultParams);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await requester.SendMessageAsyncWithRetries(request, 0);

            // Assert
            A.CallTo(
                () =>
                    httpClient.SendAsync(
                        A<HttpRequestMessage>.That.Matches(req => req.RequestUri.ToString().Equals(expectedUrl)))
            ).MustHaveHappened();
        }

        [Fact]
        public async Task GetResponseAsync_WithQueryParameterAndDefaultParameters_ShouldAppendDefaultParameters()
        {
            // Arrange
            var httpClient = A.Fake<IHttpClient>();
            var url = "test/itemName/123?parameter=value";
            var defaultParams = new NameValueCollection { { "APIKey", "keyvalue" } };
            var expectedUrl = "/test/itemName/123?parameter=value&APIKey=keyvalue";
            A.CallTo(
                    () =>
                        httpClient.SendAsync(
                            A<HttpRequestMessage>.Ignored))
                .Returns(_basicResponse);
            A.CallTo(
                    () =>
                        httpClient.Client)
                .Returns(_basicClient);
            var requester = new WebRequester(httpClient, TimeSpan.FromMilliseconds(1), defaultParams);

            // Act
            await requester.GetResponseAsync(url);

            // Assert
            A.CallTo(
                () =>
                    httpClient.SendAsync(
                        A<HttpRequestMessage>.That.Matches(req => req.RequestUri.ToString().Equals(expectedUrl)))
            ).MustHaveHappened();
        }
    }
}
