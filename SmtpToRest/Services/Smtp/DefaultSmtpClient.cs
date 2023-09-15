using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;

namespace SmtpToRest.Services.Smtp;

internal class DefaultSmtpClient : MailKit.Net.Smtp.SmtpClient, ISmtpClient
{
	public Task ConnectAsync(string? host, int port, CancellationToken cancellationToken)
	{
		return base.ConnectAsync(host, port, SecureSocketOptions.Auto, cancellationToken);
	}

	public new Task AuthenticateAsync(string? username, string? password, CancellationToken cancellationToken)
	{
		return base.AuthenticateAsync(username, password, cancellationToken);
	}

	public Task SendAsync(IMimeMessage message, CancellationToken cancellationToken)
	{
		if (message.ToMimeKitMimeMessage() is not { } mimeMessage)
		{
			throw new SmtpException("Unable to convert message to MimeKit message format");
		}
		return base.SendAsync(mimeMessage, cancellationToken);
	}
}