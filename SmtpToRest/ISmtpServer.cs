using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest
{
    public interface ISmtpServer
    {
        Task StartAsync(CancellationToken cancellationToken);
        void Shutdown();
    }
}