using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmtpToRest;
using SmtpToRest.Config;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.UseSmtpToRestDefaults()
	.AddSingleton<IConfigurationProvider>(sp =>
	{
		IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
		return new ConfigurationProvider(() => Path.Combine(env.ContentRootPath, "config"));
	});

var app = builder.Build();
app.Run();