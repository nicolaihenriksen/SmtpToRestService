using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest.Rest;

internal interface IRestClient
{
    Task<HttpResponseMessage> InvokeService(RestInput input, CancellationToken cancellationToken);
}