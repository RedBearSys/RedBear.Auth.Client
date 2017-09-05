using System.Net.Http;
using System.Threading.Tasks;

namespace RedBear.Auth.Client
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
