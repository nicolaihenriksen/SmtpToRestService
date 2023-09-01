using System;
using System.Collections.Concurrent;
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
	private Mock<ILogger> Logger { get; } = new();
	private TestConfiguration Configuration { get; } = new();
	private BlockingCollection<IMimeMessage>? MessageQueue { get; set; }
	private MockHttpMessageHandler HttpMessageHandler { get; } = new();

	public SmtpServerBackgroundServiceTests()
	{
		HttpMessageHandler.Fallback.Respond(HttpStatusCode.InternalServerError);
		Mock<IHttpClientFactory> httpClientFactory = new();
		httpClientFactory
			.Setup(f => f.CreateClient(It.IsAny<string>()))
			.Returns(new HttpClient(HttpMessageHandler));

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
			.ConfigureServices(services =>
			{
				services.AddSmtpToRest(options =>
				{
					options.ConfigurationMode = ConfigurationMode.OptionInjection;
					options.Configuration = Configuration;
					options.UseBuiltInHttpClientFactory = false;
					options.UseBuiltInMessageStoreFactory = false;
					options.UseBuiltInSmtpServerFactory = false;
				});
				services.AddSingleton(_ => httpClientFactory.Object);
				services.AddSingleton(_ => messageStoreFactory.Object);
				services.AddSingleton(_ => smtpServerFactory.Object);
			});
		HostBuilder.ConfigureLogging(loggingBuilder =>
		{
			loggingBuilder.ClearProviders();
			loggingBuilder.AddProvider(new CustomLoggerProvider(Logger.Object));
		});
	}

	public void Dispose()
	{
		_cts.Cancel();
	}

	private void StartHost(Action<IServiceCollection>? services = null)
	{
		if (Host != null)
			return;

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