using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public Task<string> GetResponseAsStringAsync(
            string pathTemplate,
            NameValueCollection parameters,
            AuthenticationHeaderValue authenticationHeader,
            int retries = 0)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var uri = UriBuilder.Build(pathTemplate, parameters);

            return GetResponseAsStringAsync(uri, retries);
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


        public async Task<string> GetResponseAsStringAsync(
            string path,
            AuthenticationHeaderValue authenticationHeader,
            int retries = 0)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = authenticationHeader;

            var response = await SendMessageAsync(request).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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


        public Task<HttpResponseMessage> GetResponseAsync(
            string pathTemplate,
            NameValueCollection parameters,
            AuthenticationHeaderValue authenticationHeader,
            int retries = 0)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var uri = UriBuilder.Build(pathTemplate, parameters);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = authenticationHeader;

            return SendMessageAsync(request);
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

        public Task<HttpResponseMessage> GetResponseAsync(
            string path,
            AuthenticationHeaderValue authenticationHeader,
            int retries = 0)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = authenticationHeader;

            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> GetResponseAsync(string path, int retries = 0)
        {
            var requestPolicy = Policy
                .Handle<TaskCanceledException>()
                .WaitAndRetryAsync(retries, retryAttempt => _retryTimeout);

            return requestPolicy.ExecuteAsync(() => PerformGetRequestAsync(path));
        }

        public Task<HttpResponseMessage> PostAsync(
            string path,
            StringContent content,
            AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> PostAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = content
            };

            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> PutAsync(
            string path,
            StringContent content,
            AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> PutAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path)
            {
                Content = content
            };

            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(
            string path,
            StringContent content,
            AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(string path, AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            request.Headers.Authorization = authenticationHeader;
            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path)
            {
                Content = content
            };

            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path);
            return SendMessageAsync(request);
        }

        public Task<HttpResponseMessage> SendMessageAsync(HttpRequestMessage request)
        {
            AddDefaultQueryParameters(request);
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> SendMessageAsyncWithRetries(HttpRequestMessage request, int retries)
        {
            AddDefaultQueryParameters(request);
            var requestPolicy = Policy
                .Handle<TaskCanceledException>()
                .WaitAndRetryAsync(retries, retryAttempt => _retryTimeout);

            return requestPolicy.ExecuteAsync(() => _client.SendAsync(request));
        }

        private Task<HttpResponseMessage> PerformGetRequestAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            AddDefaultQueryParameters(request);
            return _client.SendAsync(request);
        }

        private void AddDefaultQueryParameters(HttpRequestMessage request)
        {
            var currentUri = request.RequestUri;
            var path = currentUri.IsAbsoluteUri ? currentUri.PathAndQuery : currentUri.ToString();
            var pathWithDefaultQueryParameters = UriBuilder.Build(path, _defaultQueryParameters);
            request.RequestUri = new Uri(pathWithDefaultQueryParameters, UriKind.Relative);
        }
    }
}
