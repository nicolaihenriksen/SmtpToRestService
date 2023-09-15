using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Processing;

internal class SmtpRelayMessageProcessor : IMessageProcessor
{
	private readonly ILogger<SmtpRelayMessageProcessor> _logger;
	private readonly SmtpRelayOptions _options;
	private readonly ISmtpClientFactory _smtpClientFactory;

	public SmtpRelayMessageProcessor(ILogger<SmtpRelayMessageProcessor> logger, SmtpRelayOptions options, ISmtpClientFactory smtpClientFactory)
	{
		_logger = logger;
		_options = options;
		_smtpClientFactory = smtpClientFactory;
	}

	public async Task ProcessAsync(IMimeMessage message, CancellationToken cancellationToken)
	{
		using ISmtpClient smtpClient = _smtpClientFactory.Create();
		try
		{
			await smtpClient.ConnectAsync(_options.Host, _options.Port, cancellationToken);
			if (_options.UseSsl)
			{
				await smtpClient.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
			}
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "Unable to configure SMTP relay to {SmtpRelayHost} on port {Port} with the provided credentials", _options.Host, _options.Port);
			return;
		}
		await smtpClient.SendAsync(message, cancellationToken);
	}
}