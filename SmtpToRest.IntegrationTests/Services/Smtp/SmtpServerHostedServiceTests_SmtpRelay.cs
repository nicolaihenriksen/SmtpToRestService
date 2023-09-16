using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using SmtpToRest.Config;
using SmtpToRest.Processing;
using SmtpToRest.Services.Smtp;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace SmtpToRest.IntegrationTests.Services.Smtp;

public partial class SmtpServerHostedServiceTests
{
	private const string CategorySmtpRelay = "SmtpRelay";

	[Fact]
	[Trait(CategoryKey, CategorySmtpRelay)]
	public async Task ProcessMessages_ShouldForwardEmailWithoutAuthentication_WhenSmtpRelayEnabledWithoutAuthentication()
	{
		// Arrange
		Options.SmtpRelayOptions.Enabled = true;
		Options.SmtpRelayOptions.Host = "smtp.host.com";
		Options.SmtpRelayOptions.Port = 42;
		Options.SmtpRelayOptions.Authenticate = false;
		Mock<ISmtpClient> smtpClient = new();
		await StartHost(services =>
		{
			services.AddSingleton(smtpClient.Object);
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
		smtpClient.Verify(x => x.ConnectAsync("smtp.host.com", 42, It.IsAny<CancellationToken>()), Times.Once);
		smtpClient.Verify(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	[Trait(CategoryKey, CategorySmtpRelay)]
	public async Task ProcessMessages_ShouldForwardEmailWithAuthentication_WhenSmtpRelayEnabledWithAuthentication()
	{
		// Arrange
		Options.SmtpRelayOptions.Enabled = true;
		Options.SmtpRelayOptions.Host = "smtp.host.com";
		Options.SmtpRelayOptions.Port = 42;
		Options.SmtpRelayOptions.Authenticate = true;
		Options.SmtpRelayOptions.Username = "some.username";
		Options.SmtpRelayOptions.Password = "some.password";
		Mock<ISmtpClient> smtpClient = new();
		await StartHost(services =>
		{
			services.AddSingleton(smtpClient.Object);
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
		smtpClient.Verify(x => x.ConnectAsync("smtp.host.com", 42, It.IsAny<CancellationToken>()), Times.Once);
		smtpClient.Verify(x => x.AuthenticateAsync("some.username", "some.password", It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	[Trait(CategoryKey, CategorySmtpRelay)]
	public async Task ProcessMessages_ShouldRespectConfigurationOverridesWithoutAuthentication_WhenSpecified()
	{
		// Arrange
		Options.SmtpRelayOptions.Enabled = true;
		Options.SmtpRelayOptions.Host = "smtp.host.com";
		Options.SmtpRelayOptions.Port = 42;
		Options.SmtpRelayOptions.Authenticate = true;
		Options.SmtpRelayOptions.Username = "some.username";
		Options.SmtpRelayOptions.Password = "some.password";
		Mock<ISmtpClient> smtpClient = new();
		await StartHost(services =>
		{
			services.AddSingleton(smtpClient.Object);
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Configuration.SmtpRelay.Host = "some.other.host.com";
		Configuration.SmtpRelay.Port = 69;
		Configuration.SmtpRelay.Authenticate = false;
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
		smtpClient.Verify(x => x.ConnectAsync("some.other.host.com", 69, It.IsAny<CancellationToken>()), Times.Once);
		smtpClient.Verify(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	[Trait(CategoryKey, CategorySmtpRelay)]
	public async Task ProcessMessages_ShouldRespectConfigurationOverridesWithAuthentication_WhenSpecified()
	{
		// Arrange
		Options.SmtpRelayOptions.Enabled = true;
		Options.SmtpRelayOptions.Host = "smtp.host.com";
		Options.SmtpRelayOptions.Port = 42;
		Options.SmtpRelayOptions.Authenticate = false;
		Mock<ISmtpClient> smtpClient = new();
		await StartHost(services =>
		{
			services.AddSingleton(smtpClient.Object);
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Configuration.SmtpRelay.Host = "some.other.host.com";
		Configuration.SmtpRelay.Port = 69;
		Configuration.SmtpRelay.Authenticate = true;
		Configuration.SmtpRelay.Username = "some.other.username";
		Configuration.SmtpRelay.Password = "some.other.password";
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
		smtpClient.Verify(x => x.ConnectAsync("some.other.host.com", 69, It.IsAny<CancellationToken>()), Times.Once);
		smtpClient.Verify(x => x.AuthenticateAsync("some.other.username", "some.other.password", It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	[Trait(CategoryKey, CategorySmtpRelay)]
	public async Task ProcessMessages_ShouldRespectConfigurationMappingOverridesWithoutAuthentication_WhenSpecified()
	{
		// Arrange
		Options.SmtpRelayOptions.Enabled = true;
		Options.SmtpRelayOptions.Host = "smtp.host.com";
		Options.SmtpRelayOptions.Port = 42;
		Options.SmtpRelayOptions.Authenticate = true;
		Options.SmtpRelayOptions.Username = "some.username";
		Options.SmtpRelayOptions.Password = "some.password";
		Mock<ISmtpClient> smtpClient = new();
		await StartHost(services =>
		{
			services.AddSingleton(smtpClient.Object);
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			SmtpRelay = new()
			{
				Host = "some.other.host.com",
				Port = 69,
				Authenticate = false,
			}
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
		smtpClient.Verify(x => x.ConnectAsync("some.other.host.com", 69, It.IsAny<CancellationToken>()), Times.Once);
		smtpClient.Verify(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	[Trait(CategoryKey, CategorySmtpRelay)]
	public async Task ProcessMessages_ShouldRespectConfigurationMappingOverridesWithAuthentication_WhenSpecified()
	{
		// Arrange
		Options.SmtpRelayOptions.Enabled = true;
		Options.SmtpRelayOptions.Host = "smtp.host.com";
		Options.SmtpRelayOptions.Port = 42;
		Options.SmtpRelayOptions.Authenticate = false;
		Mock<ISmtpClient> smtpClient = new();
		await StartHost(services =>
		{
			services.AddSingleton(smtpClient.Object);
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			SmtpRelay = new()
			{
				Host = "some.other.host.com",
				Port = 69,
				Authenticate = true,
				Username = "some.other.username",
				Password = "some.other.password"
			}
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
		smtpClient.Verify(x => x.ConnectAsync("some.other.host.com", 69, It.IsAny<CancellationToken>()), Times.Once);
		smtpClient.Verify(x => x.AuthenticateAsync("some.other.username", "some.other.password", It.IsAny<CancellationToken>()), Times.Once);
	}
}