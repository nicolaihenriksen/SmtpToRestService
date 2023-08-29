using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmtpServer.Storage;

namespace SmtpToRest.Services.Smtp;

internal class DefaultMessageStoreFactory : IMessageStoreFactory
{
	private readonly IServiceProvider _serviceProvider;

	public DefaultMessageStoreFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

    public IMessageStore Create(BlockingCollection<IMimeMessage> messageQueue)
    {
        return new SimpleMessageStore(_serviceProvider.GetRequiredService<ILogger<SimpleMessageStore>>(), messageQueue);
    }
}