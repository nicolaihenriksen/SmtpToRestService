using System.Text.Json;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class ContentDecorator : DecoratorBase, IRestInputDecorator
{
	public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
	{
		string? content = null;
		if (mapping.Content is string stringContent)
			content = stringContent;
		else
			content = mapping.Content != null ? JsonSerializer.Serialize(mapping.Content) : restInput.Content;

		// TODO: Handle placeholder replacements
		restInput.Content = content;
	}
}