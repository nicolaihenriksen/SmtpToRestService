using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Processing;

internal class SmtpRelayMessageProcessor : IMessageProcessor
{
	private readonly ILogger<SmtpRelayMessageProcessor> _logger;
	private readonly SmtpRelayOptions _options;
	private readonly ISmtpClientFactory _smtpClientFactory;
	private readonly IConfiguration _configuration;
	private readonly IConfigurationMappingKeyExtractor _mappingKeyExtractor;

	public SmtpRelayMessageProcessor(ILogger<SmtpRelayMessageProcessor> logger,
		SmtpRelayOptions options,
		ISmtpClientFactory smtpClientFactory,
		IConfiguration configuration,
		IConfigurationMappingKeyExtractor mappingKeyExtractor)
	{
		_logger = logger;
		_options = options;
		_smtpClientFactory = smtpClientFactory;
		_configuration = configuration;
		_mappingKeyExtractor = mappingKeyExtractor;
	}

	public async Task ProcessAsync(IMimeMessage message, CancellationToken cancellationToken)
	{
		using ISmtpClient smtpClient = _smtpClientFactory.Create();
		string? host = _options.Host;
		int port = _options.Port;
		string? username = _options.Username;
		string? password = _options.Password;
		bool useSsl = _options.UseSsl;

		string key = _mappingKeyExtractor.ExtractKey(message);
		if (_configuration.TryGetMapping(key, out ConfigurationMapping? mapping) &&
			mapping?.SmtpRelay is { } smtpRelayConfiguration)
		{
			host = smtpRelayConfiguration.Host ?? host;
			port = smtpRelayConfiguration.Port ?? port;
			username = smtpRelayConfiguration.Username ?? username;
			password = smtpRelayConfiguration.Password ?? password;
			useSsl = smtpRelayConfiguration.UseSsl ?? useSsl;
		}

		try
		{
			await smtpClient.ConnectAsync(host, port, cancellationToken);
			if (useSsl)
			{
				await smtpClient.AuthenticateAsync(username, password, cancellationToken);
			}
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Unable to configure SMTP relay to {SmtpRelayHost} on port {Port} with the provided credentials", host, port);
			return;
		}
		await smtpClient.SendAsync(message, cancellationToken);
	}
}