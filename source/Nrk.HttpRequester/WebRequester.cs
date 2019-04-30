﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Nrk.HttpRequester
{
    public class WebRequester : IWebRequester
    {
        private readonly IHttpClient _client;
        private readonly NameValueCollection _defaultQueryParameters;
        private readonly IList<Action<HttpRequestMessage>> _requestModifiers;

        public WebRequester(IHttpClient client,
            NameValueCollection defaultQueryParameters = null,
            IEnumerable<Action<HttpRequestMessage>> beforeRequestActions = null)
        {
            _client = client;
            _defaultQueryParameters = defaultQueryParameters ?? new NameValueCollection();
            _requestModifiers = beforeRequestActions?.ToList() ?? new List<Action<HttpRequestMessage>>();
        }

        private WebRequester CopyWith(IHttpClient client = null, TimeSpan? retryTimeout = null,
            NameValueCollection defaultqueryParameters = null,
            IEnumerable<Action<HttpRequestMessage>> requestModifiers = null)
        {
            return new WebRequester(
                client ?? _client,
                defaultqueryParameters ?? _defaultQueryParameters,
                requestModifiers ?? _requestModifiers
            );
        }

        /// <summary>
        /// Creates a copy of this WebRequester, with an additional requestModifier
        /// </summary>
        /// <param name="requestModifier">An Action for modifying the request message before send.</param>
        /// <returns>a copy of this WebRequester, with an additional requestModifier</returns>
        public WebRequester With(Action<HttpRequestMessage> requestModifier)
        {
            if (requestModifier == null)
            {
                throw new ArgumentNullException(nameof(requestModifier));
            }
            return CopyWith(requestModifiers: _requestModifiers.Concat(new[] {requestModifier}));
        }

        public async Task<string> GetResponseAsStringAsync(string pathTemplate, NameValueCollection parameters, AuthenticationHeaderValue authenticationHeader)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var uri = UriBuilder.Build(pathTemplate, parameters);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = authenticationHeader;

            var response = await SendMessageAsync(request).ConfigureAwait(false);

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> GetResponseAsStringAsync(string pathTemplate, NameValueCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var uri = UriBuilder.Build(pathTemplate, parameters);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await SendMessageAsync(request).ConfigureAwait(false);

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }


        public async Task<string> GetResponseAsStringAsync(string path, AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = authenticationHeader;

            var response = await SendMessageAsync(request).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<string> GetResponseAsStringAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await SendMessageAsync(request).ConfigureAwait(false);
            if (response == null)
            {
                return string.Empty;
            }

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }


        public async Task<HttpResponseMessage> GetResponseAsync(
            string pathTemplate,
            NameValueCollection parameters,
            AuthenticationHeaderValue authenticationHeader)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var uri = UriBuilder.Build(pathTemplate, parameters);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = authenticationHeader;

            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> GetResponseAsync(string pathTemplate, NameValueCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var url = UriBuilder.Build(pathTemplate, parameters);
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> GetResponseAsync(
            string path,
            AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Authorization = authenticationHeader;

            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> GetResponseAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);

            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(
            string path,
            StringContent content,
            AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = content
            };

            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PutAsync(
            string path,
            StringContent content,
            AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PutAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, path)
            {
                Content = content
            };

            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteAsync(
            string path,
            StringContent content,
            AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            request.Headers.Authorization = authenticationHeader;
            request.Content = content;
            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path, AuthenticationHeaderValue authenticationHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            request.Headers.Authorization = authenticationHeader;
            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path, StringContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path)
            {
                Content = content
            };

            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string path)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            return await SendMessageAsync(request).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> SendMessageAsync(HttpRequestMessage request)
        {
            AddDefaultQueryParameters(request);
            return await _client.SendAsync(request).ConfigureAwait(false);
        }

        private void AddDefaultQueryParameters(HttpRequestMessage request)
        {
            foreach (var action in _requestModifiers)
            {
                action(request);
            }
            var currentUri = request.RequestUri;
            var path = currentUri.IsAbsoluteUri ? currentUri.PathAndQuery : currentUri.ToString();
            var pathWithDefaultQueryParameters = UriBuilder.Build(path, _defaultQueryParameters);
            request.RequestUri = new Uri(pathWithDefaultQueryParameters, UriKind.Relative);
        }
    }
}
