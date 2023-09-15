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
		SmtpRelayOptions options = new(_options);

		// Optionally override with settings from the configuration
		options.Override(_configuration.SmtpRelay);

		// Optionally override with settings from the configuration mapping
		string key = _mappingKeyExtractor.ExtractKey(message);
		if (_configuration.TryGetMapping(key, out ConfigurationMapping? mapping))
			options.Override(mapping!.SmtpRelay);

		if (!options.Enabled)
			return;

		using ISmtpClient smtpClient = _smtpClientFactory.Create();
		try
		{
			await smtpClient.ConnectAsync(options.Host, options.Port, cancellationToken);
			if (options.UseSsl)
			{
				await smtpClient.AuthenticateAsync(options.Username, options.Password, cancellationToken);
			}
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Unable to configure SMTP relay to {SmtpRelayHost} on port {Port} with the provided credentials", options.Host, options.Port);
			return;
		}
		await smtpClient.SendAsync(message, cancellationToken);
	}
}