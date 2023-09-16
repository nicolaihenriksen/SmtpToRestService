using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpToRest.Config;
using SmtpToRest.Processing;

namespace SmtpToRest.Services.Smtp;

internal class SmtpServerHostedService : IHostedService
{
    public event EventHandler<MessageProcessedEventArgs>? MessageProcessed;

    private readonly ILogger<SmtpServerHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<IMessageProcessor> _additionalMessageProcessors;
    private readonly IMessageProcessorInternal _messageProcessor;
    private readonly IMessageStore _messageStore;
    private readonly ISmtpServerFactory _smtpServerFactory;
    private ISmtpServer? _smtpServer;

    public SmtpServerHostedService(ILogger<SmtpServerHostedService> logger,
	    IConfiguration configuration,
	    IEnumerable<IMessageProcessor> additionalMessageProcessors,
	    IMessageProcessorInternal messageProcessor,
	    IMessageStore messageStore,
	    ISmtpServerFactory smtpServerFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _additionalMessageProcessors = additionalMessageProcessors;
        _messageProcessor = messageProcessor;
        _messageStore = messageStore;
        _smtpServerFactory = smtpServerFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
	    ISmtpServerOptions options = new SmtpServerOptionsBuilder()
		    .ServerName(_configuration.SmtpHost ?? "localhost")
		    .Port(_configuration.SmtpPorts ?? new[] {25, 587})
		    .Build();

	    _logger.LogInformation("Starting SMTP server on {Ports}", string.Join(", ", options.Endpoints.Select(e => e.Endpoint.Port)));

		ServiceProvider serviceProvider = new();
	    _messageStore.MessageReceived += OnMessageReceived;

	    serviceProvider.Add(_messageStore);
	    _smtpServer = _smtpServerFactory.Create(options, serviceProvider);
	    _smtpServer.StartAsync(cancellationToken);
	    _logger.LogInformation("SMTP server started");
		return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
	    _smtpServer?.Shutdown();
	    _logger.LogInformation("Stopped SMTP server");
	    return Task.CompletedTask;
    }

	private async void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
		_logger.LogDebug("Processing message...");
        CancellationToken cancellationToken = CancellationToken.None;
		foreach (IMessageProcessor processor in _additionalMessageProcessors)
		{
			try
			{
				await processor.ProcessAsync(e.Message, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing message. AdditionalMessageProcessor type='{MessageProcessorType}', Error='{Error}'", processor.GetType().FullName, ex.Message);
			}
		}
		ProcessResult result = await _messageProcessor.ProcessAsync(e.Message, cancellationToken);
		if (!result.IsSuccess)
		{
			_logger.LogError("Error processing message. Error='{Error}'", result.Error);
		}
		MessageProcessed?.Invoke(this, new(result));
	}
}