using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using SmtpServer;
using SmtpToRest.Config;
using SmtpToRest.Processing;
using SmtpToRest.Services.Smtp;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using IMessageStore = SmtpToRest.Services.Smtp.IMessageStore;

namespace SmtpToRest.IntegrationTests.Services.Smtp;

public partial class SmtpServerHostedServiceTests : IDisposable
{
	private const string CategoryKey = "Category";

	private readonly CancellationTokenSource _cts = new();
	private readonly ManualResetEventSlim _sync = new();

	private IHostBuilder HostBuilder { get; }
	private IHost? Host { get; set; }
	private IHttpClientFactory HttpClientFactory { get; }
	private Mock<ILogger> Logger { get; } = new();
	private SmtpToRestOptions Options { get; } = new();
	private TestConfiguration Configuration { get; } = new();
	private MockHttpMessageHandler HttpMessageHandler { get; } = new();
	private Mock<IMessageStore> MessageStore { get; } = new();

	public SmtpServerHostedServiceTests()
	{
		Options.Configuration = Configuration;
		Options.ConfigurationMode = ConfigurationMode.OptionInjection;
		Options.UseBuiltInHttpClientFactory = false;
		Options.UseBuiltInMessageStore = false;
		Options.UseBuiltInSmtpServerFactory = false;

		HttpMessageHandler.Fallback.Respond(HttpStatusCode.InternalServerError);
		Mock<IHttpClientFactory> httpClientFactory = new();
		httpClientFactory
			.Setup(f => f.CreateClient(It.IsAny<string>()))
			.Returns(new HttpClient(HttpMessageHandler));
		HttpClientFactory = httpClientFactory.Object;

		Mock<ISmtpServer> smtpServer = new();
		smtpServer
			.Setup(s => s.StartAsync(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);
		Mock<ISmtpServerFactory> smtpServerFactory = new();
		smtpServerFactory
			.Setup(f => f.Create(It.IsAny<ISmtpServerOptions>(), It.IsAny<IServiceProvider>()))
			.Returns(smtpServer.Object);

		HostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
			.ConfigureLogging((context, loggingBuilder) =>
			{
				loggingBuilder
					.ClearProviders()
					.AddProvider(new CustomLoggerProvider(Logger.Object))
					.AddFilter(_ => true);
			})
			.ConfigureServices(services =>
			{
				services.AddSingleton(_ => MessageStore.Object);
				services.AddSingleton(_ => smtpServerFactory.Object);
			});
	}

	public void Dispose()
	{
		_cts.Cancel();
		GC.SuppressFinalize(this);
	}

	private async Task StartHost(Action<IServiceCollection>? services = null, Action<IHttpClientBuilder>? httpConfig = null)
	{
		if (Host != null)
			return;

		HostBuilder.ConfigureServices(s =>
		{
			s.AddSmtpToRest(options =>
			{
				options.ConfigurationMode = Options.ConfigurationMode;
				options.Configuration = Options.Configuration;
				options.UseBuiltInHttpClientFactory = Options.UseBuiltInHttpClientFactory;
				options.UseBuiltInMessageStore = Options.UseBuiltInMessageStore;
				options.UseBuiltInSmtpServerFactory = Options.UseBuiltInSmtpServerFactory;
				options.HttpClientName = Options.HttpClientName;
				options.SmtpRelayOptions.Enabled = Options.SmtpRelayOptions.Enabled;
				options.SmtpRelayOptions.Host = Options.SmtpRelayOptions.Host;
				options.SmtpRelayOptions.Port = Options.SmtpRelayOptions.Port;
				options.SmtpRelayOptions.UseSsl = Options.SmtpRelayOptions.UseSsl;
				options.SmtpRelayOptions.Username = Options.SmtpRelayOptions.Username;
				options.SmtpRelayOptions.Password = Options.SmtpRelayOptions.Password;
			}, httpConfig ?? (_ => { }));
			if (!Options.UseBuiltInHttpClientFactory)
			{
				s.AddSingleton(_ => HttpClientFactory);
			}
		});

		if (services != null)
		{
			HostBuilder.ConfigureServices(services);
		}
		Host = HostBuilder.Build();
		await Host.StartAsync(_cts.Token);
	}

	private Mock<IMimeMessage> Arrange(string fromAddress, ConfigurationMapping mapping)
	{
		if (string.IsNullOrWhiteSpace(mapping.Key))
		{
			mapping.Key = fromAddress;
		}
		Mock<IMimeMessage> message = new();
		message
			.SetupGet(m => m.FirstFromAddress)
			.Returns(fromAddress);

		Configuration.AddMapping(fromAddress, mapping);
		return message;
	}

	private async Task<ProcessResult?> SendMessageAsync(IMimeMessage message)
	{
		await StartHost();
		ManualResetEventSlim sync = new();
		ProcessResult? result = default;
		SmtpServerHostedService smtpServer = (SmtpServerHostedService) Host!.Services.GetRequiredService<IHostedService>();
		smtpServer.MessageProcessed += OnMessageProcessed;
		MessageStore.Raise(m => m.MessageReceived += null, new MessageReceivedEventArgs(message));

		if (Debugger.IsAttached)
			sync.Wait(-1);
		else
			sync.Wait(TimeSpan.FromSeconds(2));

		smtpServer.MessageProcessed -= OnMessageProcessed;
		return result;

		void OnMessageProcessed(object? sender, MessageProcessedEventArgs e)
		{
			result = e.ProcessResult;
			sync.Set();
		}
	}

	private void AssertLog(LogLevel logLevel, string message, Type? exceptionType = null)
	{
		if (exceptionType != null)
		{
			Logger.Verify(l => l.Log(logLevel,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(message)),
				It.Is<Exception>(ex => ex.GetType().IsAssignableTo(exceptionType)),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.AtLeastOnce);
		}
		else
		{
			Logger.Verify(l => l.Log(logLevel,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(message)),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.AtLeastOnce);
		}
	}
}