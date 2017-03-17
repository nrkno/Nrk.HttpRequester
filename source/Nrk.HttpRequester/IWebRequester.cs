using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Nrk.HttpRequester
{
    public interface IWebRequester
    {
        Task<string> GetResponseAsStringAsync(string path, int retries = 0);
        Task<string> GetResponseAsStringAsync(string pathTemplate, NameValueCollection parameters, int retries = 0);
        Task<HttpResponseMessage> GetResponseAsync(string path, int retries = 0);
        Task<HttpResponseMessage> GetResponseAsync(string pathTemplate, NameValueCollection parameters, int retries = 0);
        Task<HttpResponseMessage> PostAsync(string path, StringContent content, AuthenticationHeaderValue authenticationHeader);
        Task<HttpResponseMessage> PostAsync(string path, StringContent content);
        Task<HttpResponseMessage> PutAsync(string path, StringContent content, AuthenticationHeaderValue authenticationHeader);
        Task<HttpResponseMessage> PutAsync(string path, StringContent content);
        Task<HttpResponseMessage> DeleteAsync(string path, StringContent content, AuthenticationHeaderValue authenticationHeader);
        Task<HttpResponseMessage> DeleteAsync(string path, AuthenticationHeaderValue authenticationHeader);
        Task<HttpResponseMessage> DeleteAsync(string path, StringContent content);
        Task<HttpResponseMessage> DeleteAsync(string path);
        /// <summary>
        /// Intended for use cases not covered by default methods provided
        /// Direct access to underlying http client
        /// </summary>
        Task<HttpResponseMessage> SendMessageAsync(HttpRequestMessage request);
        Task<HttpResponseMessage> SendMessageAsyncWithRetries(HttpRequestMessage request, int retries);
    }


}
