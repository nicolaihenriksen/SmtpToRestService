using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.IntegrationTests;

internal class CustomConfigurationMappingKeyExtractor : IConfigurationMappingKeyExtractor
{
    public string ExtractKey(IMimeMessage message)
    {
        return message.FirstToAddress ?? string.Empty;
    }
}