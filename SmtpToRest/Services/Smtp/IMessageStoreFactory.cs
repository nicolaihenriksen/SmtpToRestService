using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SmtpServer.Storage;

namespace SmtpToRest.Services.Smtp;

internal interface IMessageStoreFactory
{
    IMessageStore Create(ILogger<SmtpServerBackgroundService> logger, BlockingCollection<IMimeMessage> messageQueue);
}