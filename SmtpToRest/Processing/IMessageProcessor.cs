using System.Threading;
using System.Threading.Tasks;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Processing;

internal interface IMessageProcessor
{
    Task<ProcessResult> ProcessAsync(IMimeMessage message, CancellationToken cancellationToken);
}