using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest.Rest;

public interface IRestClient
{
    Task<HttpResponseMessage> InvokeService(RestInput input, CancellationToken cancellationToken);
}

public interface IDefaultRestClient : IRestClient
{ }