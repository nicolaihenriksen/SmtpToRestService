using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SmtpServer.Storage;

namespace SmtpToRestService
{
    internal interface IMessageStoreFactory
    {
        IMessageStore Create(ILogger<SmtpServerBackgroundService> logger, BlockingCollection<IMimeMessage> messageQueue);
    }
}