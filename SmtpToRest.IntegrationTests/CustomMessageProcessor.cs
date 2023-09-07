using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmtpToRest.Processing;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.IntegrationTests;

internal class CustomMessageProcessor : IMessageProcessor
{
    private readonly ILogger<CustomMessageProcessor> _logger;

    public CustomMessageProcessor(ILogger<CustomMessageProcessor> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(IMimeMessage message, CancellationToken cancellationToken)
    {
        _logger.LogDebug("CustomMessageProcessor pre-processing message");
        return Task.CompletedTask;
    }
}