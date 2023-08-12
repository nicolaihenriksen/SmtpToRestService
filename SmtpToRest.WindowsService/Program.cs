using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmtpToRest;

namespace SmtpToRestService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddSingleton<IConfiguration, Configuration>()
                        .AddSingleton<IConfigurationFileReader, DefaultConfigurationFileReader>()
                        .AddSingleton<IMessageStoreFactory, DefaultMessageStoreFactory>()
                        .AddSingleton<ISmtpServerFactory, DefaultSmtpServerFactory>()
                        .AddSingleton<IRestClient, RestClient>()
                        .AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>()
                        .AddHostedService<SmtpServerBackgroundService>();
                })
                .UseWindowsService();
    }
}