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
                    services.AddSingleton<SmtpToRest.IConfiguration, Configuration>();
                    services.AddSingleton<IConfigurationFileReader, DefaultConfigurationFileReader>();
                    services.AddSingleton<SmtpToRest.IMessageStoreFactory, DefaultMessageStoreFactory>();
                    services.AddSingleton<ISmtpServerFactory, DefaultSmtpServerFactory>();
                    services.AddSingleton<IRestClient, RestClient>();
                    services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();
                    services.AddHostedService<SmtpServerBackgroundService>();
                })
                .UseWindowsService();
    }
}