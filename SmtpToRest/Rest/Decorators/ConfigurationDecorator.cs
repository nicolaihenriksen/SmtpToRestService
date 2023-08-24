using System;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

public class ConfigurationDecorator : DecoratorBase, IRestInputDecorator
{
	private readonly IConfiguration _configuration;

	public ConfigurationDecorator(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public RestInput Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
	{
		restInput.ApiToken = _configuration.ApiToken;
		if (Enum.TryParse(_configuration.HttpMethod, true, out HttpMethod parsedHttpMethod))
			restInput.HttpMethod = parsedHttpMethod;
		restInput.Endpoint = _configuration.Endpoint;
		return restInput;
	}
}