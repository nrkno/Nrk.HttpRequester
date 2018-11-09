using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nrk.HttpRequester
{
    public interface IHttpClientBuilder
    {
        /// <summary>
        /// Sets User-Agent
        /// </summary>
        /// <returns></returns>
        IHttpClientBuilder WithUserAgent(UserAgent userAgent);

        /// <summary>
        /// Sets the number of milliseconds to wait before the request times out.
        /// </summary>
        IHttpClientBuilder WithTimeout(TimeSpan timeout);

        /// <summary>
        /// Sets a DelegatingHandler.
        /// Can be reused to set several different Handlers in a pipeline.
        /// </summary>
        IHttpClientBuilder WithHandler(DelegatingHandler handler);

        /// <summary>
        /// Sets the CacheHandler
        /// </summary>
        IHttpClientBuilder WithCacheHandler(DelegatingHandler cacheHandler);

        /// <summary>
        /// Sets a WebRequestHandler configurator
        /// </summary>
        IHttpClientBuilder WithRequestHandlerConfigurator(Action<WebRequestHandler> configurator);

        /// <summary>
        /// Sets Default HttpRequestHeaders
        /// </summary>
        IHttpClientBuilder WithDefaultRequestHeaders(Dictionary<string, string> headers);

        /// <summary>
        /// Sets ConnectionLeaseTimeout in milliseconds on the <see cref="ServicePoint"/> for the baseUrl.
        /// </summary>
        IHttpClientBuilder WithConnectionLeaseTimeout(int connectionLeaseTimeout);

        /// <summary>
        /// Creates the <see cref="HttpClient"/>
        /// </summary>
        IHttpClient Create();
    }

    public static class WebRequestHttpClientFactory
    {
        private class HttpClientBuilder : IHttpClientBuilder
        {
            private UserAgent _userAgent;
            private readonly Uri _baseUrl;
            private TimeSpan? _timeout;
            private readonly List<DelegatingHandler> _handlers = new List<DelegatingHandler>();
            private readonly List<Action<WebRequestHandler>> _requestHandlerConfigurators = new List<Action<WebRequestHandler>>();
            private Dictionary<string, string> _defaultHeaders;
            private DelegatingHandler _cacheHandler;
            private int _connectionLeaseTimeout;

            public HttpClientBuilder(Uri baseUrl)
            {
                if (baseUrl == null)
                {
                    throw new ArgumentNullException(nameof(baseUrl));
                }

                _baseUrl = baseUrl;
            }

            public IHttpClientBuilder WithUserAgent(UserAgent userAgent)
            {
                _userAgent = userAgent;
                return this;
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

            public IHttpClientBuilder WithConnectionLeaseTimeout(int connectionLeaseTimeout)
            {
                _connectionLeaseTimeout = connectionLeaseTimeout;
                return this;
            }

            public IHttpClientBuilder WithRequestHandlerConfigurator(Action<WebRequestHandler> configurator)
            {
                _requestHandlerConfigurators.Add(configurator);
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
                _requestHandlerConfigurators.ForEach((configurator) => configurator(webRequestHandler));
                
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

                foreach (var header in _defaultHeaders ?? Enumerable.Empty<KeyValuePair<string, string>>())
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                foreach (var productInfoHeaderValue in _userAgent?.ToProductInfoHeaderValues() ?? Enumerable.Empty<ProductInfoHeaderValue>())
                {
                    client.DefaultRequestHeaders.UserAgent.Add(productInfoHeaderValue);
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

            public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            {
                return await Client.SendAsync(request).ConfigureAwait(false);
            }
        }
    }
}
