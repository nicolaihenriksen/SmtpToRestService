using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SmtpToRest.Config;

namespace SmtpToRest.Rest;

public interface IRestClient
{
    Task<HttpResponseMessage> InvokeService(ConfigurationMapping mapping, CancellationToken? cancellationToken = null);
}