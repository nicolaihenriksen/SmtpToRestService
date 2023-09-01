using Microsoft.Extensions.Logging;

namespace SmtpToRest.IntegrationTests;

internal class CustomLoggerProvider : ILoggerProvider
{
    private readonly ILogger _logger;

    public CustomLoggerProvider(ILogger logger)
    {
        _logger = logger;
    }

    public void Dispose() { }

    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }
}