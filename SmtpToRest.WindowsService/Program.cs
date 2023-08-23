using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmtpToRest.Config;

namespace SmtpToRest.WindowsService;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
	            services
		            .UseSmtpToRestDefaults()
		            .AddSingleton<IConfigurationProvider>(sp => new ConfigurationProvider(() => Path.Combine(System.AppContext.BaseDirectory)));
			})
            .UseWindowsService();
}