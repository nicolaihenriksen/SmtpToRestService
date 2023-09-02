using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmtpToRest;
using SmtpToRest.Config;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
	.ConfigureServices((_, services) =>
	{
		services
			.AddSmtpToRest()
			.AddSingleton<IConfigurationProvider>(sp => new ConfigurationProvider(() => Path.Combine(System.AppContext.BaseDirectory)));
	})
	.UseWindowsService();

IHost app = builder.Build();
app.Run();