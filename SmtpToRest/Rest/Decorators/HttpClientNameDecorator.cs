using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class HttpClientNameDecorator : DecoratorBase, IRestInputDecorator
{
	public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
	{
		restInput.HttpClientName = mapping.CustomHttpClientName;
	}
}