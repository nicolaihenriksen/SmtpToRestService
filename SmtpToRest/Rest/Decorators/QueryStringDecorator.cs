using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class QueryStringDecorator : DecoratorBase, IRestInputDecorator
{
	public RestInput Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
	{
		if (restInput.HttpMethod == HttpMethod.Get)
		{
			// TODO: Handle placeholder replacements
			restInput.QueryString = mapping.QueryString;
		}
		return restInput;
	}
}