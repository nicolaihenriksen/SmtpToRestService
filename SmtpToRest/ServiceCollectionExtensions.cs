using System;
using Microsoft.Extensions.DependencyInjection;
using SmtpToRest.Config;
using SmtpToRest.Processing;
using SmtpToRest.Rest;
using SmtpToRest.Rest.Decorators;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSmtpToRest(this IServiceCollection services, Action<SmtpToRestOptions>? configure = null, Action<IHttpClientBuilder>? httpConfiguration = null)
	{
		SmtpToRestOptions options = new();
		configure?.Invoke(options);

		if (options.UseBuiltInDecorators)
			services.AddDefaultDecorators();

		string httpClientName = options.HttpClientName ?? SmtpToRestOptions.DefaultHttpClientName;
		if (options.UseBuiltInHttpClientFactory)
		{
			IHttpClientBuilder httpClientBuilder = services.AddHttpClient(httpClientName);
			httpConfiguration?.Invoke(httpClientBuilder);
		}

		if (options.UseBuiltInMessageStore)
			services.AddSingleton<IMessageStore, DefaultMessageStore>();

		if (options.UseBuiltInSmtpServerFactory)
			services.AddSingleton<ISmtpServerFactory, DefaultSmtpServerFactory>();

		if (options.UseBuiltInMessageProcessor)
			services.AddSingleton<IMessageProcessorInternal, DefaultMessageProcessor>();

		switch (options.ConfigurationMode)
		{
			case ConfigurationMode.ConfigurationProvider:
				services
					.AddSingleton<IConfiguration, Configuration>()
					.AddSingleton<IConfigurationFileReader, DefaultConfigurationFileReader>();
				break;
			case ConfigurationMode.ServiceInjection:
				// Do nothing, the caller will inject their own instance into the service collection
				break;
			case ConfigurationMode.OptionInjection:
				if (options.Configuration is null)
					throw new InvalidOperationException($"When {nameof(options.ConfigurationMode)} is {ConfigurationMode.OptionInjection}, the {nameof(options.Configuration)} must be supplied.");
				services.AddSingleton(_ => options.Configuration);
				break;
			case ConfigurationMode.None:
			default:
				throw new InvalidOperationException($"Invalid {nameof(options.ConfigurationMode)} supplied.");
		}

		services
			.AddSingleton<IConfigurationMappingKeyExtractor, DefaultConfigurationMappingKeyExtractor>()
			.AddSingleton<IRestInputDecoratorInternal, AggregateDecorator>()
			.AddSingleton<IRestClient, RestClient>()
			.AddHostedService<SmtpServerHostedService>()
			.AddSingleton<IHttpClientConfiguration>(_ => new HttpClientConfiguration(httpClientName))
			.AddSingleton(options.SmtpRelayOptions)
			.AddTransient<ISmtpClient, DefaultSmtpClient>()
			.AddSingleton<ISmtpClientFactory, DefaultSmtpClientFactory>()
			.AddSingleton<IMessageProcessor, SmtpRelayMessageProcessor>();

		return services;
	}

	private static void AddDefaultDecorators(this IServiceCollection services)
	{
		services
			.AddSingleton<IRestInputDecorator, ConfigurationDecorator>()
			.AddSingleton<IRestInputDecorator, EndpointOverridesDecorator>()
			.AddSingleton<IRestInputDecorator, ServiceDecorator>()
			.AddSingleton<IRestInputDecorator, QueryStringDecorator>()
			.AddSingleton<IRestInputDecorator, ContentDecorator>();
	}
}