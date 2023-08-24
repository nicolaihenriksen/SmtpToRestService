using System.Threading;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;
using SmtpToRest.Rest;
using SmtpToRest.Rest.Decorators;

namespace SmtpToRest.Processing;

internal class DefaultMessageProcessor : IMessageProcessor
{
	private readonly IConfiguration _configuration;
	private readonly IRestClient _restClient;
	private readonly IRestInputDecoratorInternal _decorator;
	private readonly ILogger<DefaultMessageProcessor> _logger;

	public DefaultMessageProcessor(ILogger<DefaultMessageProcessor> logger, IConfiguration configuration, IRestClient restClient, IRestInputDecoratorInternal decorator)
	{
		_logger = logger;
		_configuration = configuration;
		_restClient = restClient;
		_decorator = decorator;
	}

	public async Task<ProcessResult> ProcessAsync(IMimeMessage message, CancellationToken cancellationToken)
	{
		if (_configuration.TryGetMapping(message.Address, out var mapping) && mapping is not null)
		{
			try
			{
				var input = _decorator.Decorate(new RestInput(), mapping, message);
				await _restClient.InvokeService(input, cancellationToken);
				return ProcessResult.Success();
			}
			catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
			{
				_logger.LogError(ex, "Error invoking REST service for mapping. Key='{MappingKey}'", mapping.Key);
				return ProcessResult.Failure($"Error invoking REST service for mapping. Key='{mapping.Key}'");
			}
		}
		return ProcessResult.Failure($"Unable to find mapping for address: '{message.Address}'");
	}
}