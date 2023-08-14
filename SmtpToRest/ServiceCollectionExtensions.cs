using Microsoft.Extensions.DependencyInjection;
using SmtpToRest.Config;
using SmtpToRest.Processing;
using SmtpToRest.Rest;
using SmtpToRest.Rest.Decorators;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection UseSmtpToRestDefaults(this IServiceCollection services)
	{
		services
			.UseDecorators()
			.AddSingleton<IConfiguration, Configuration>()
			.AddSingleton<IMessageStoreFactory, DefaultMessageStoreFactory>()
			.AddSingleton<ISmtpServerFactory, DefaultSmtpServerFactory>()
			.AddSingleton<IMessageProcessor, DefaultMessageProcessor>()
			.AddSingleton<IRestClient, RestClient>()
			.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>()
			.AddHostedService<SmtpServerBackgroundService>();
		return services;
	}

	private static IServiceCollection UseDecorators(this IServiceCollection services)
	{
		// TODO: UGH! I need to find a better approach for this
		services
			.AddSingleton<ConfigurationDecorator>()
			.AddSingleton<EndpointOverridesDecorator>()
			.AddSingleton<QueryStringDecorator>()
			.AddSingleton<JsonPostDataDecorator>()
			.AddSingleton<IRestInputDecorator>(provider => new AggregateDecorator(
					provider.GetRequiredService<ConfigurationDecorator>(),
					provider.GetRequiredService<EndpointOverridesDecorator>(),
					provider.GetRequiredService<QueryStringDecorator>(),
					provider.GetRequiredService<JsonPostDataDecorator>()));

		return services;
	}
}