using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class JsonPostDataDecorator : DecoratorBase, IRestInputDecorator
{
	public RestInput Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
	{
		if (restInput.HttpMethod == HttpMethod.Post)
		{
			// TODO: Handle placeholder replacements
			restInput.JsonPostData = mapping.JsonPostData;
		}
		return restInput;
	}
}