using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Nrk.HttpRequester
{
    public class WebRequester : IWebRequester
    {
        private readonly IHttpClient _client;
        private readonly TimeSpan _retryTimeout;
        private readonly NameValueCollection _defaultQueryParameters;

        public WebRequester(IHttpClient client, TimeSpan retryTimeout, NameValueCollection defaultQueryParameters = null)
        {
            _client = client;
            _retryTimeout = retryTimeout;
            _defaultQueryParameters = defaultQueryParameters ?? new NameValueCollection();
        }

        public WebRequester(IHttpClient client)
        {
            _client = client;
            _retryTimeout = TimeSpan.FromSeconds(3);
            _defaultQueryParameters = new NameValueCollection();
        }

        public Task<string> GetResponseAsStringAsync(string pathTemplate, NameValueCollection parameters, int retries = 0)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var uri = UriBuilder.Build(pathTemplate, parameters);

            return GetResponseAsStringAsync(uri, retries);
        }

        public async Task<string> GetResponseAsStringAsync(string path, int retries = 0)
        {
            var response = await GetResponseAsync(path, retries).ConfigureAwait(false);
            if (response == null)
            {
                return string.Empty;
            }

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public Task<HttpResponseMessage> GetResponseAsync(string pathTemplate, NameValueCollection parameters, int retries = 0)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var url = UriBuilder.Build(pathTemplate, parameters);

            return GetResponseAsync(url, retries);
        }
        public Task<HttpResponseMessage> GetResponseAsync(string path, IList<TimeSpan> retries)
        {
            var urlWithParameters = UriBuilder.Build(path, _defaultQueryParameters);
            var retryQueue = new Queue<TimeSpan>(retries);
            var delay = retryQueue.Dequeue();
            var requestPolicy = Policy
                .Handle<TaskCanceledException>()
                .RetryAsync(retries.Count -1, (ex, ct) =>
                {
                    if (retryQueue.Count > 0) delay = retryQueue.Dequeue();
                }); 

            return requestPolicy.ExecuteAsync(() => PerformGetRequestAsyncWithTimeout(urlWithParameters, delay));
        }

        public Task<HttpResponseMessage> GetResponseAsync(string path, int retries = 0)
        {
            var urlWithParameters = UriBuilder.Build(path, _defaultQueryParameters);
            var requestPolicy = Policy
                .Handle<TaskCanceledException>()
                .WaitAndRetryAsync(retries, retryAttempt => _retryTimeout);

            return requestPolicy.ExecuteAsync(() => PerformGetRequestAsync(urlWithParameters));
        }

        public Task<HttpResponseMessage> PostAsync(string path, StringContent content, AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> PostAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = content
            };

            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> PutAsync(string path, StringContent content, AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> PutAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path)
            {
                Content = content
            };

            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(string path, StringContent content, AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(string path, AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            request.Headers.Authorization = authenticationHeader;
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path)
            {
                Content = content
            };

            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path);
            return _client.SendAsync(request);
        }
      
        public Task<HttpResponseMessage> SendMessageAsync(HttpRequestMessage request)
        {
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> SendMessageAsyncWithRetries(HttpRequestMessage request, int retries)
        {
            var requestPolicy = Policy
                .Handle<TaskCanceledException>()
                .WaitAndRetryAsync(retries, retryAttempt => _retryTimeout);

            return requestPolicy.ExecuteAsync(() => _client.SendAsync(request));
        }

        private Task<HttpResponseMessage> PerformGetRequestAsyncWithTimeout(string path, TimeSpan timeout)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);
            var tsk = _client.SendAsync(request, new HttpCompletionOption(), cts.Token);
            return tsk;
        }

         private Task<HttpResponseMessage> PerformGetRequestAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            return _client.SendAsync(request);
        }
    }
}
