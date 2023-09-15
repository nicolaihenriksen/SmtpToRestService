using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpServer;
using SmtpServer.Protocol;

namespace SmtpToRest.Services.Smtp;

internal class DefaultMessageStore : IMessageStore
{
	public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

	private readonly ILogger<DefaultMessageStore> _logger;

	public DefaultMessageStore(ILogger<DefaultMessageStore> logger)
	{
		_logger = logger;
	}

	public async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
	{
		_logger.LogDebug("Reading incoming message...");
		await using var stream = new MemoryStream();
		SequencePosition position = buffer.GetPosition(0);
		while (buffer.TryGet(ref position, out var memory))
		{
			await stream.WriteAsync(memory, cancellationToken);
		}
		stream.Position = 0;
		try
		{
			MimeMessage message = await MimeMessage.LoadAsync(stream, cancellationToken);
			MessageReceived?.Invoke(this, new MessageReceivedEventArgs(new MimeMessageAdapter(message)));
		}
		catch
		{
			_logger.LogError("Unable to parse incoming message");
		}
		return SmtpResponse.Ok;
	}
}