using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest
{
    public interface IRestClient
    {
        Task<HttpResponseMessage> InvokeService(ConfigurationMapping mapping, CancellationToken? cancellationToken = null);
    }
}