using System;
using System.Collections.Specialized;
using System.Net.Http;
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

        private Task<HttpResponseMessage> PerformGetRequestAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> PostDataAsync(string path, string userAccessToken, StringContent content)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> PutDataAsync(string path, string userAccessToken, StringContent content)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteAsync(string path, string userAccessToken, StringContent content)
        {
            throw new NotImplementedException();
        }
    }
}
