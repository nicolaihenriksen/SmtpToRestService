using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest;

public class SmtpServerAdapter : ISmtpServer
{
    private readonly SmtpServer.SmtpServer _adaptee;

    public SmtpServerAdapter(SmtpServer.SmtpServer adaptee)
    {
        _adaptee = adaptee;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _adaptee.StartAsync(cancellationToken);
    }

    public void Shutdown()
    {
        _adaptee.Shutdown();
    }
}