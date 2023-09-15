using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Config;

internal class DefaultConfigurationMappingKeyExtractor : IConfigurationMappingKeyExtractor
{
	public string ExtractKey(IMimeMessage message)
	{
		return message.FirstFromAddress ?? string.Empty;
	}
}