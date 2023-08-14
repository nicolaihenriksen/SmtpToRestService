using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmtpToRest;
using SmtpToRest.Config;
using SmtpToRest.Docker;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.UseSmtpToRestDefaults()
	.AddSingleton<IConfigurationFileReader, DockerConfigurationFileReader>();

var app = builder.Build();
app.Run();