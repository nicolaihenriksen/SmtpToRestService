using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmtpToRest;
using SmtpToRest.Config;

var builder = Host.CreateDefaultBuilder(args)
	.ConfigureServices((_, services) =>
	{
		services
			.UseSmtpToRestDefaults()
			.AddSingleton<IConfigurationProvider>(sp => new ConfigurationProvider(() => Path.Combine(System.AppContext.BaseDirectory)));
	})
	.UseWindowsService();

var app = builder.Build();
app.Run();