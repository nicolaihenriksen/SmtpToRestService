using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class QueryStringDecorator : DecoratorBase, IRestInputDecorator
{
	public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
	{
		// TODO: Handle placeholder replacements
		restInput.QueryString = mapping.QueryString;
	}
}