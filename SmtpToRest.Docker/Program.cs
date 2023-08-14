using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmtpToRest;
using SmtpToRest.Docker;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<IConfiguration, Configuration>()
    .AddSingleton<IConfigurationFileReader, DockerConfigurationFileReader>()
    .AddSingleton<IMessageStoreFactory, DefaultMessageStoreFactory>()
    .AddSingleton<ISmtpServerFactory, DefaultSmtpServerFactory>()
    .AddSingleton<IRestClient, RestClient>()
    .AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>()
    .AddHostedService<SmtpServerBackgroundService>();

var app = builder.Build();
app.Run();