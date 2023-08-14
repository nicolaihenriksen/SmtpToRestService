using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest.Services.Smtp;

public interface ISmtpServer
{
    Task StartAsync(CancellationToken cancellationToken);
    void Shutdown();
}