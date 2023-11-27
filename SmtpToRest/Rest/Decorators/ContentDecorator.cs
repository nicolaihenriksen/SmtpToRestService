using System.Text.Encodings.Web;
using System.Text.Json;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class ContentDecorator : DecoratorBase, IRestInputDecorator
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
		// Needed in order for special characters like '+' to be allowed in the deserialized JSON.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

	public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
	{
		string? content = restInput.Content;
		if (mapping.Content is string stringContent)
			content = stringContent;
		else if (mapping.Content is not null)
			content = JsonSerializer.Serialize(mapping.Content, JsonSerializerOptions);

        restInput.Content = content;
	}
}