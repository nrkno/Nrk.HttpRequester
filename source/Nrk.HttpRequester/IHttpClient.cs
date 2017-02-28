using System.Net.Http;
using System.Threading.Tasks;

namespace Nrk.HttpRequester
{
    public interface IHttpClient
    {
        HttpClient Client { get; }
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
