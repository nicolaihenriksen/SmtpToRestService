using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Config;

public interface IConfigurationMappingKeyExtractor
{
	string ExtractKey(IMimeMessage message);
}