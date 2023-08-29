using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace SmtpToRest.Services.Smtp;

internal class SimpleMessageStore : MessageStore
{
    private readonly ILogger<SimpleMessageStore> _logger;
    private readonly BlockingCollection<IMimeMessage> _messageQueue;

    public SimpleMessageStore(ILogger<SimpleMessageStore> logger, BlockingCollection<IMimeMessage> messageQueue)
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