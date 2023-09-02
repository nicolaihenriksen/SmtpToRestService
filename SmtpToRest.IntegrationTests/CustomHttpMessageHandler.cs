using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SmtpToRest.IntegrationTests;

internal class CustomHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger<CustomHttpMessageHandler> _logger;

    public CustomHttpMessageHandler(ILogger<CustomHttpMessageHandler> logger)
    {
        _logger = logger;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("CustomHttpMessageHandler short-circuiting the HTTP Client");
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unused) { ReasonPhrase = "CustomHttpMessageHandler terminated the request" });
    }
}