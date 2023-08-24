using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest.Services.Smtp;

internal interface ISmtpServer
{
    Task StartAsync(CancellationToken cancellationToken);
    void Shutdown();
}