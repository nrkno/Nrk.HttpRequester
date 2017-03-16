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

        public Task<HttpResponseMessage> GetResponseAsync(string path, int retries = 0)
        {
            var urlWithParameters = UriBuilder.Build(path, _defaultQueryParameters);
            var requestPolicy = Policy
                .Handle<TaskCanceledException>()
                .WaitAndRetryAsync(retries, retryAttempt => _retryTimeout);

            return requestPolicy.ExecuteAsync(() => PerformGetRequestAsync(urlWithParameters));
        }

        public Task<HttpResponseMessage> PostAsync(string path, string authenticationScheme, string accessToken, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path);
            SetAuthenticationHeader(request, authenticationScheme, accessToken);
            request.Content = content;
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> PutAsync(string path, string authenticationScheme, string accessToken, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path);
            SetAuthenticationHeader(request, authenticationScheme, accessToken);
            request.Content = content;
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> DeleteAsync(string path, string authenticationScheme, string accessToken, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            SetAuthenticationHeader(request, authenticationScheme, accessToken);
            request.Content = content;
            return _client.SendAsync(request);
        }

        private Task<HttpResponseMessage> PerformGetRequestAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            return _client.SendAsync(request);
        }

        private static void SetAuthenticationHeader(HttpRequestMessage request, string authenticationScheme, string accessToken)
        {
            if (string.IsNullOrEmpty(authenticationScheme) || string.IsNullOrEmpty(accessToken)) return;
            request.Headers.Authorization = new AuthenticationHeaderValue(authenticationScheme, accessToken);
        }

    }
}
