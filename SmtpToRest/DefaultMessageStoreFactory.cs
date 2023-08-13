using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SmtpServer.Storage;

namespace SmtpToRest;

public class DefaultMessageStoreFactory : IMessageStoreFactory
{
    public IMessageStore Create(ILogger<SmtpServerBackgroundService> logger, BlockingCollection<IMimeMessage> messageQueue)
    {
        return new SimpleMessageStore(logger, messageQueue);
    }
}