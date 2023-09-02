using System;
using System.Collections.Concurrent;
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
using SmtpServer.Storage;
using SmtpToRest.Config;
using SmtpToRest.Processing;
using SmtpToRest.Services.Smtp;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using IMessageStoreFactory = SmtpToRest.Services.Smtp.IMessageStoreFactory;

namespace SmtpToRest.IntegrationTests.Services.Smtp;

public partial class SmtpServerBackgroundServiceTests : IDisposable
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
	private BlockingCollection<IMimeMessage>? MessageQueue { get; set; }
	private MockHttpMessageHandler HttpMessageHandler { get; } = new();

	public SmtpServerBackgroundServiceTests()
	{
		Options.Configuration = Configuration;
		Options.ConfigurationMode = ConfigurationMode.OptionInjection;
		Options.UseBuiltInHttpClientFactory = false;
		Options.UseBuiltInMessageStoreFactory = false;
		Options.UseBuiltInSmtpServerFactory = false;

		HttpMessageHandler.Fallback.Respond(HttpStatusCode.InternalServerError);
		Mock<IHttpClientFactory> httpClientFactory = new();
		httpClientFactory
			.Setup(f => f.CreateClient(It.IsAny<string>()))
			.Returns(new HttpClient(HttpMessageHandler));
		HttpClientFactory = httpClientFactory.Object;

		Mock<IMessageStoreFactory> messageStoreFactory = new();
		messageStoreFactory
			.Setup(f => f.Create(It.IsAny<BlockingCollection<IMimeMessage>>()))
			.Callback((BlockingCollection<IMimeMessage> messageQueue) =>
			{
				MessageQueue = messageQueue;
				_sync.Set();
			})
			.Returns(new Mock<IMessageStore>().Object);

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
				services.AddSingleton(_ => messageStoreFactory.Object);
				services.AddSingleton(_ => smtpServerFactory.Object);
			});
	}

	public void Dispose()
	{
		_cts.Cancel();
	}

	private void StartHost(Action<IServiceCollection>? services = null, Action<IHttpClientBuilder>? httpConfig = null)
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
				options.UseBuiltInMessageStoreFactory = Options.UseBuiltInMessageStoreFactory;
				options.UseBuiltInSmtpServerFactory = Options.UseBuiltInSmtpServerFactory;
				options.HttpClientName = Options.HttpClientName;
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
		Host.StartAsync(_cts.Token).Wait();
		_sync.Wait();
	}

	private Mock<IMimeMessage> Arrange(string address, ConfigurationMapping mapping)
	{
		if (string.IsNullOrWhiteSpace(mapping.Key))
		{
			mapping.Key = address;
		}
		Mock<IMimeMessage> message = new();
		message
			.SetupGet(m => m.Address)
			.Returns(address);

		Configuration.AddMapping(address, mapping);
		return message;
	}

	private ProcessResult? SendMessage(IMimeMessage message)
	{
		StartHost();
		ManualResetEventSlim sync = new();
		ProcessResult? result = default;
		SmtpServerBackgroundService smtpServer = (SmtpServerBackgroundService) Host!.Services.GetRequiredService<IHostedService>();
		smtpServer.MessageProcessed += OnMessageProcessed;
		MessageQueue!.Add(message);

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