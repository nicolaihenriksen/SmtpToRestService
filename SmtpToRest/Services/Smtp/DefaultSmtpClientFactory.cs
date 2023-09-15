using System;
using Microsoft.Extensions.DependencyInjection;

namespace SmtpToRest.Services.Smtp;

internal class DefaultSmtpClientFactory : ISmtpClientFactory
{
	private readonly IServiceProvider _serviceProvider;

	public DefaultSmtpClientFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public ISmtpClient Create()
	{
		return _serviceProvider.GetRequiredService<ISmtpClient>();
	}
}