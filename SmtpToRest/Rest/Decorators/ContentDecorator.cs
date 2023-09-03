using System.Text.Json;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class ContentDecorator : DecoratorBase, IRestInputDecorator
{
	public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
	{
		string? content = restInput.Content;
		if (mapping.Content is string stringContent)
			content = stringContent;
		else if (mapping.Content is not null)
			content = JsonSerializer.Serialize(mapping.Content);

		// TODO: Handle placeholder replacements
		restInput.Content = content;
	}
}