using System.Collections.Concurrent;
using SmtpServer.Storage;

namespace SmtpToRest.Services.Smtp;

public interface IMessageStoreFactory
{
    IMessageStore Create(BlockingCollection<IMimeMessage> messageQueue);
}