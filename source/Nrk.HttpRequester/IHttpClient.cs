using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nrk.HttpRequester
{
    public interface IHttpClient
    {
        HttpClient Client { get; }
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken);
    }
}
