using System.Threading;
using System.Threading.Tasks;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Processing;

public interface IMessageProcessor
{
    Task<ProcessResult> ProcessAsync(IMimeMessage message, CancellationToken cancellationToken);
}