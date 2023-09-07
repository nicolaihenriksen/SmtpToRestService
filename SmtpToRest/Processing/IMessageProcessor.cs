using System.Threading;
using System.Threading.Tasks;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Processing;

public interface IMessageProcessor
{
    Task ProcessAsync(IMimeMessage message, CancellationToken cancellationToken);
}

internal interface IMessageProcessorInternal
{
    Task<ProcessResult> ProcessAsync(IMimeMessage message, CancellationToken cancellationToken);
}