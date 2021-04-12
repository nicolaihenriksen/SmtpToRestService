using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpToRestService
{
    internal class SmtpServerBackgroundService : BackgroundService
    {
        public event EventHandler MessageProcessed;
        
        private readonly ILogger<SmtpServerBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMessageStoreFactory _messageStoreFactory;
        private readonly ISmtpServerFactory _smtpServerFactory;
        private readonly IRestClient _restClient;
        private readonly BlockingCollection<IMimeMessage> _messageQueue = new BlockingCollection<IMimeMessage>();
        private ISmtpServer _smtpServer;

        public SmtpServerBackgroundService(ILogger<SmtpServerBackgroundService> logger, IConfiguration configuration, IMessageStoreFactory messageStoreFactory, ISmtpServerFactory smtpServerFactory, IRestClient restClient)
        {
            _logger = logger;
            _configuration = configuration;
            _messageStoreFactory = messageStoreFactory;
            _smtpServerFactory = smtpServerFactory;
            _restClient = restClient;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(FormattableString.Invariant($"Starting..."));
            try
            {
                var options = new SmtpServerOptionsBuilder()
                    .ServerName("localhost")
                    .Port(25, 587)
                    .Build();

                var serviceProvider = new ServiceProvider();
                var messageStore = _messageStoreFactory.Create(_logger, _messageQueue);
                serviceProvider.Add(messageStore);
                _smtpServer = _smtpServerFactory.Create(options, serviceProvider);

                var messageProcessingTask = Task.Factory.StartNew(async () => await ProcessMessages(stoppingToken), stoppingToken).ContinueWith(a => { }, stoppingToken);
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

        private async Task ProcessMessages(CancellationToken cancellationToken)
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable(cancellationToken))
            {
                _logger.LogDebug(FormattableString.Invariant($"Processing message..."));
                if (_configuration.TryGetMapping(message.Address, out var mapping))
                {
                    try
                    {
                        await _restClient.InvokeService(mapping, cancellationToken);
                    }
                    catch when (!cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogError(FormattableString.Invariant($"Error invoking REST service for mapping. Key='{mapping.Key}'"));
                    }
                }
                MessageProcessed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    internal class SimpleMessageStore : MessageStore
    {
        private readonly ILogger<SmtpServerBackgroundService> _logger;
        private readonly BlockingCollection<IMimeMessage> _messageQueue;

        public SimpleMessageStore(ILogger<SmtpServerBackgroundService> logger, BlockingCollection<IMimeMessage> messageQueue)
        {
            _logger = logger;
            _messageQueue = messageQueue;
        }
        
        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            _logger.LogDebug(FormattableString.Invariant($"Reading incoming message..."));
            await using var stream = new MemoryStream();
            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                await stream.WriteAsync(memory, cancellationToken);
            }
            stream.Position = 0;
            try
            { 
                var message = await MimeMessage.LoadAsync(stream, cancellationToken);
                _messageQueue.Add(new MimeMessageAdapter(message), cancellationToken);
            }
            catch
            {
                _logger.LogError(FormattableString.Invariant($"Unable to parse incoming message"));
            }
            return SmtpResponse.Ok;
        }
    }
}