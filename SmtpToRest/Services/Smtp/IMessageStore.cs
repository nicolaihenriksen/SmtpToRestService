using System;

namespace SmtpToRest.Services.Smtp;

public interface IMessageStore : SmtpServer.Storage.IMessageStore
{
	event EventHandler<MessageReceivedEventArgs> MessageReceived;
}

public class MessageReceivedEventArgs : EventArgs
{
	public IMimeMessage Message { get; }
	
	public MessageReceivedEventArgs(IMimeMessage message)

	{
		Message = message;
	}
}
