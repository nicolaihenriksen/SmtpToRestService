using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRestService
{
    internal interface ISmtpServer
    {
        Task StartAsync(CancellationToken cancellationToken);
        void Shutdown();
    }
}