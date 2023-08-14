using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmtpToRest.Config;
using SmtpToRest.Docker;
using SmtpToRest.Processing;
using SmtpToRest.Rest;
using SmtpToRest.Services.Smtp;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<IConfiguration, Configuration>()
    .AddSingleton<IConfigurationFileReader, DockerConfigurationFileReader>()
    .AddSingleton<IMessageStoreFactory, DefaultMessageStoreFactory>()
    .AddSingleton<ISmtpServerFactory, DefaultSmtpServerFactory>()
    .AddSingleton<IMessageProcessor, DefaultMessageProcessor>()
	.AddSingleton<IRestClient, RestClient>()
    .AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>()
    .AddHostedService<SmtpServerBackgroundService>();

var app = builder.Build();
app.Run();