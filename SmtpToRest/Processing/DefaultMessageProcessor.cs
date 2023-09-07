using System.Threading;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;
using SmtpToRest.Rest;
using SmtpToRest.Rest.Decorators;

namespace SmtpToRest.Processing;

internal class DefaultMessageProcessor : IMessageProcessorInternal
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
		if (message.Address is null)
		{
			return ProcessResult.Failure("No address found in message");
		}

		if (_configuration.TryGetMapping(message.Address, out ConfigurationMapping? mapping) && mapping is not null)
		{
			try
			{
				RestInput input = new();
				_decorator.Decorate(input, mapping, message);
				HttpResponseMessage response = await _restClient.InvokeService(input, cancellationToken);
				return response.IsSuccessStatusCode ? ProcessResult.Success() : ProcessResult.Failure(response.ReasonPhrase ?? "Unknown error");
			}
			catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
			{
				_logger.LogError(ex, "Error invoking REST service for mapping. Key='{MappingKey}'", mapping.Key);
				return ProcessResult.Failure($"Error invoking REST service for mapping. Key='{mapping.Key}'");
			}
		}
		return ProcessResult.Failure($"No mapping found for address: '{message.Address}'");
	}
}