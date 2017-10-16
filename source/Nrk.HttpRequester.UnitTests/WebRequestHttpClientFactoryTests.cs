using System;
using System.Collections.Generic;
using System.Net;
using Nrk.HttpRequester.UnitTests.TestData;
using Shouldly;
using Xunit;

namespace Nrk.HttpRequester.UnitTests
{
    public class WebRequestHttpClientFactoryTests
    {
        [Fact]
        public void Create_NullBaseUrl_ShouldThrowArgumentNullException()
        {
            // Act
            var ex = Record.Exception(() => WebRequestHttpClientFactory.Configure(null, Some.UserAgent).Create());
            // Assert
            ex.ShouldBeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Create_NullUserAgent_ShouldThrowArgumentNullException()
        {
            // Act
            var ex = Record.Exception(() => WebRequestHttpClientFactory.Configure(Some.Uri, null).Create());
            // Assert
            ex.ShouldBeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Create_GivenTimeout_ShouldSetTimeoutForHttpClient()
        {
            // Arrange
            var baseUri = new Uri("http://fake.api.com");
            var timeout = TimeSpan.FromSeconds(20);

            // Act
            var httpClient = WebRequestHttpClientFactory.Configure(baseUri, Some.UserAgent).WithTimeout(timeout).Create();

            // Assert
            httpClient.Client.Timeout.ShouldBe(timeout);
        }

        [Fact]
        public void Create_GivenHttpCache_ShouldSetHttpCacheForHttpClient()
        {
            var baseUri = new Uri("http://fake.api.com");
            var timeout = TimeSpan.FromSeconds(20);
            var httpClient = WebRequestHttpClientFactory.Configure(baseUri, Some.UserAgent).WithTimeout(timeout).Create();
            httpClient.Client.Timeout.ShouldBe(timeout);
        }

        [Fact]
        public void Create_GivenDefaultRequestHeaders_ShouldSetRequestHeadersForHttpClient()
        {
            // Arrange
            var baseUri = new Uri("http://fake.api.com");
            var requestHeaders = new Dictionary<string, string> { { "headerKey", "headerValue" } };

            // Act
            var httpClient = WebRequestHttpClientFactory.Configure(baseUri, Some.UserAgent).WithDefaultRequestHeaders(requestHeaders).Create();
            
            // Assert
            httpClient.Client.DefaultRequestHeaders.Contains("headerKey").ShouldBeTrue();
        }

        [Fact]
        public void Create_GivenConnectionLeaseTimeout_ShouldSetConnectionLeaseOnBaseUrl()
        {
            // Arrange
            var baseUri = new Uri("http://fake.api.com");
            const int oneMinute = 60 * 1000;
            // Act
            var httpClient = WebRequestHttpClientFactory.Configure(baseUri, Some.UserAgent).WithConnectionLeaseTimeout(60*1000).Create();

            // Assert
            var connectionLeaseTimeout = ServicePointManager.FindServicePoint(baseUri).ConnectionLeaseTimeout;
            connectionLeaseTimeout.ShouldBe(oneMinute);
        }

        [Fact]
        public void Create_WithoutConnectionLeaseTimeout_ShouldGiveDefaultConnectionLeaseOnBaseUrl()
        {
            // Arrange
            var baseUri = new Uri("http://fake.api.com");
            const int defaultConnectionLease = -1;
            // Act
            var httpClient = WebRequestHttpClientFactory.Configure(baseUri, Some.UserAgent).Create();

            // Assert
            var connectionLeaseTimeout = ServicePointManager.FindServicePoint(baseUri).ConnectionLeaseTimeout;
            connectionLeaseTimeout.ShouldBe(defaultConnectionLease);
        }

        [Fact]
        public void Create_GivenConnectionLeaseTimeout_ShouldSetConnectionLeaseOnBaseUrlAndPath()
        {
            // Arrange
            var baseUri = new Uri("http://fake.api.com");
            var baseUriWithPath = new Uri(baseUri, "medium/tv?queryParameter=3");

            const int oneMinute = 60 * 1000;
            // Act
            var httpClient = WebRequestHttpClientFactory.Configure(baseUri, Some.UserAgent).WithConnectionLeaseTimeout(60 * 1000).Create();

            // Assert
            var connectionLeaseTimeout = ServicePointManager.FindServicePoint(baseUriWithPath).ConnectionLeaseTimeout;
            connectionLeaseTimeout.ShouldBe(oneMinute);
        }

    }
}
