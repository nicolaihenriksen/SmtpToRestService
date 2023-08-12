using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmtpToRest;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton<IConfiguration, Configuration>()
    .AddSingleton<IConfigurationFileReader, DefaultConfigurationFileReader>()
    .AddSingleton<IMessageStoreFactory, DefaultMessageStoreFactory>()
    .AddSingleton<ISmtpServerFactory, DefaultSmtpServerFactory>()
    .AddSingleton<IRestClient, RestClient>()
    .AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>()
    .AddHostedService<SmtpServerBackgroundService>();

var app = builder.Build();
app.Run();