using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nrk.HttpRequester
{
    public interface IHttpClientBuilder
    {
        IHttpClientBuilder WithTimeout(TimeSpan timeout);

        IHttpClientBuilder WithHandler(DelegatingHandler handler);

        IHttpClientBuilder WithCacheHandler(DelegatingHandler cacheHandler);

        IHttpClientBuilder WithDefaultRequestHeaders(Dictionary<string, string> headers);

        IHttpClientBuilder WithConnectionLeaseTimeout(int connectionLeaseTimeout);

        IHttpClient Create();
    }

    public static class WebRequestHttpClientFactory
    {
        private class HttpClientBuilder : IHttpClientBuilder
        {
            private readonly Uri _baseUrl;
            private TimeSpan? _timeout;
            private readonly List<DelegatingHandler> _handlers = new List<DelegatingHandler>();
            private Dictionary<string, string> _defaultHeaders;
            private DelegatingHandler _cacheHandler;
            private int _connectionLeaseTimeout;

            public HttpClientBuilder(Uri baseUrl)
            {
                if (baseUrl == null) throw new ArgumentNullException(nameof(baseUrl));

                _baseUrl = baseUrl;
            }

            public IHttpClientBuilder WithTimeout(TimeSpan timeout)
            {
                _timeout = timeout;
                return this;
            }

            public IHttpClientBuilder WithHandler(DelegatingHandler handler)
            {
                _handlers.Add(handler);
                return this;
            }

            public IHttpClientBuilder WithCacheHandler(DelegatingHandler cacheHandler)
            {
                _cacheHandler = cacheHandler;
                return this;
            }

            public IHttpClientBuilder WithDefaultRequestHeaders(Dictionary<string, string> headers)
            {
                _defaultHeaders = headers;
                return this;
            }

            // ConnectionLeaseTimeout is the number of milliseconds after which an active ServicePoint connection is closed.
            // Ref. http://byterot.blogspot.no/2016/07/singleton-httpclient-dns.html
            public IHttpClientBuilder WithConnectionLeaseTimeout(int connectionLeaseTimeout)
            {
                _connectionLeaseTimeout = connectionLeaseTimeout;
                return this;
            }

            public IHttpClient Create()
            {
                var webRequestHandler = new WebRequestHandler();
                if (webRequestHandler.SupportsAutomaticDecompression)
                {
                    webRequestHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
#if DEBUG
                webRequestHandler.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
#endif

                var client = _cacheHandler == null
                    ? HttpClientFactory.Create(webRequestHandler, _handlers.ToArray())
                    : HttpClientFactory.Create(_cacheHandler, _handlers.ToArray());

                client.BaseAddress = _baseUrl;

                if (_connectionLeaseTimeout != 0)
                {
                    var servicePoint = ServicePointManager.FindServicePoint(_baseUrl);
                    servicePoint.ConnectionLeaseTimeout = _connectionLeaseTimeout;
                }

                if (_timeout.HasValue)
                {
                    client.Timeout = _timeout.Value;
                }

                if (_defaultHeaders == null) return new HttpClientWrapper(client);

                foreach (var header in _defaultHeaders)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                return new HttpClientWrapper(client);
            }
        }

        public static IHttpClientBuilder Configure(Uri baseUri)
        {
            return new HttpClientBuilder(baseUri);
        }

        /// <summary>
        /// This class was made to make unit testing easier when HttpClient is used.
        /// </summary>
        private class HttpClientWrapper : IHttpClient
        {
            public HttpClient Client { get; }

            public HttpClientWrapper(HttpClient client)
            {
                Client = client;
            }

            public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            {
                return Client.SendAsync(request);
            }
        }
    }
}
