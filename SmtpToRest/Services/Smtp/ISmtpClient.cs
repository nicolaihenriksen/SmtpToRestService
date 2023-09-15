using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest.Services.Smtp;

internal interface ISmtpClient : IDisposable
{
	Task ConnectAsync(string? host, int port, CancellationToken cancellationToken);
	Task AuthenticateAsync(string? username, string? password, CancellationToken cancellationToken);
	Task SendAsync(IMimeMessage message, CancellationToken cancellationToken);
}