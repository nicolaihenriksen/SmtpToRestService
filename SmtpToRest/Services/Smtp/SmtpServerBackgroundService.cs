using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpToRest.Config;
using SmtpToRest.Processing;

namespace SmtpToRest.Services.Smtp;

internal class SmtpServerBackgroundService : BackgroundService
{
    public event EventHandler<MessageProcessedEventArgs>? MessageProcessed;

    private readonly ILogger<SmtpServerBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<IMessageProcessor> _additionalMessageProcessors;
    private readonly IMessageProcessorInternal _messageProcessor;
    private readonly IMessageStoreFactory _messageStoreFactory;
    private readonly ISmtpServerFactory _smtpServerFactory;
    private readonly BlockingCollection<IMimeMessage> _messageQueue = new();
    private ISmtpServer? _smtpServer;

    public SmtpServerBackgroundService(ILogger<SmtpServerBackgroundService> logger,
	    IConfiguration configuration,
	    IEnumerable<IMessageProcessor> additionalMessageProcessors,
	    IMessageProcessorInternal messageProcessor,
	    IMessageStoreFactory messageStoreFactory,
	    ISmtpServerFactory smtpServerFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _additionalMessageProcessors = additionalMessageProcessors;
        _messageProcessor = messageProcessor;
        _messageStoreFactory = messageStoreFactory;
        _smtpServerFactory = smtpServerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(FormattableString.Invariant($"Starting..."));
        try
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName(_configuration.SmtpHost ?? "localhost")
                .Port(_configuration.SmtpPorts ?? new[] { 25, 587 })
                .Build();

            var serviceProvider = new ServiceProvider();
            var messageStore = _messageStoreFactory.Create(_messageQueue);
            serviceProvider.Add(messageStore);
            _smtpServer = _smtpServerFactory.Create(options, serviceProvider);

            var messageProcessingTask = Task.Factory.StartNew(async () => await ProcessMessagesAsync(stoppingToken), stoppingToken).ContinueWith(a => { }, stoppingToken);
            var serverTask = _smtpServer.StartAsync(stoppingToken);
            await Task.WhenAll(messageProcessingTask, serverTask);
        }
        finally
        {
            _logger.LogInformation(FormattableString.Invariant($"Stopped"));
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _smtpServer?.Shutdown();
        return Task.CompletedTask;
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        foreach (IMimeMessage message in _messageQueue.GetConsumingEnumerable(cancellationToken))
        {
			_logger.LogDebug(FormattableString.Invariant($"Processing message..."));
			foreach (IMessageProcessor processor in _additionalMessageProcessors)
			{
				try
				{
					await processor.ProcessAsync(message, cancellationToken);
				}
                catch(Exception ex)
				{
					_logger.LogError(ex, "Error processing message. AdditionalMessageProcessor type='{MessageProcessorType}', Error='{Error}'", processor.GetType().FullName, ex.Message);
				}
			}
			ProcessResult result = await _messageProcessor.ProcessAsync(message, cancellationToken);
			if (!result.IsSuccess)
			{
				_logger.LogError("Error processing message. Error='{Error}'", result.Error);
			}
			MessageProcessed?.Invoke(this, new MessageProcessedEventArgs(result));
		}
    }
}