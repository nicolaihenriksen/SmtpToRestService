using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmtpToRest.Rest;

namespace SmtpToRest.IntegrationTests;

public class CustomRestClient : IRestClient
{
    private readonly ILogger<CustomRestClient> _logger;
    private readonly IRestClient _defaultRestClient;

    public CustomRestClient(ILogger<CustomRestClient> logger, IDefaultRestClient defaultRestClient)
    {
        _logger = logger;
        _defaultRestClient = defaultRestClient;
    }
    public Task<HttpResponseMessage> InvokeService(RestInput input, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Custom rest client decorator");
        return _defaultRestClient.InvokeService(input, cancellationToken);
    }
}