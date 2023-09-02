using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmtpToRest;
using SmtpToRest.Config;
using System.IO;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddSmtpToRest()
	.AddSingleton<IConfigurationProvider>(sp =>
	{
		IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
		return new ConfigurationProvider(() => Path.Combine(env.ContentRootPath, "config"));
	});

WebApplication app = builder.Build();
app.Run();