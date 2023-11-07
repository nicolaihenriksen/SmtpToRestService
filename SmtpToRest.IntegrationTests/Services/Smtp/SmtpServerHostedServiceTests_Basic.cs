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
using Microsoft.Extensions.Logging;
using Xunit;

namespace SmtpToRest.IntegrationTests.Services.Smtp;

public partial class SmtpServerHostedServiceTests
{
	private const string CategoryBasic = "Basic";

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public async Task ProcessMessages_ShouldReturnFailedResult_WhenMessageDoesNotHaveSenderAddress()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Mock<IMimeMessage> message = new();
		message
			.SetupGet(m => m.FirstFromAddress)
			.Returns((string)null!);
		MockedRequest request = HttpMessageHandler.Expect(HttpMethod.Get, Configuration.Endpoint!)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		Assert.NotNull(result);
		result.IsSuccess.Should().BeFalse();
		HttpMessageHandler.GetMatchCount(request).Should().Be(0);
		result.Error.Should().Contain("No address found in message");
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public async Task ProcessMessages_ShouldReturnFailedResult_WhenNoMappingIsFound()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Mock<IMimeMessage> message = new();
		message
			.SetupGet(m => m.FirstFromAddress)
			.Returns("sender@somewhere.com");
		MockedRequest request = HttpMessageHandler.Expect(HttpMethod.Get, Configuration.Endpoint!)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		Assert.NotNull(result);
		result.IsSuccess.Should().BeFalse();
		HttpMessageHandler.GetMatchCount(request).Should().Be(0);
		result.Error.Should().Contain("No mapping found for");
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public async Task ProcessMessages_ShouldSucceed_WhenMatchingMappingIsFound()
	{
		// Arrange
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler.Expect(HttpMethod.Get, Configuration.Endpoint!)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public async Task ProcessMessages_ShouldIncludeApiToken_WhenSuppliedInConfiguration()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Configuration.ApiToken = "someToken";
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint!)
			.With(r => r.Headers?.Authorization is { Scheme: "Bearer", Parameter: "someToken" })
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public async Task ProcessMessages_ShouldRespectOverriddenValues_WhenOverriddenInMapping()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Configuration.ApiToken = "someToken";
		ConfigurationMapping mapping = new()
		{
			CustomHttpMethod = Rest.HttpMethod.Post.ToString(),
			CustomEndpoint = "http://overriddenendpoint",
			CustomApiToken = "someOverriddenToken"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Post, "http://overriddenendpoint")
			.With(r => r.Headers?.Authorization is { Scheme: "Bearer", Parameter: "someOverriddenToken" })
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public async Task ProcessMessages_ShouldAppendService_WhenSuppliedInMapping()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			Service = "someService"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, $"{Configuration.Endpoint}/someService")
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public async Task StartHost_ShouldLog()
	{
		// Act
		await StartHost();

		// Assert
		AssertLog(LogLevel.Information, "Starting");
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public async Task StartHost_ShouldStartSmtpServer()
	{
		// Act
		await StartHost();

		// Assert
		SmtpServer.Verify(s => s.StartAsync(It.IsAny<CancellationToken>()), Times.Once());
		AssertLog(LogLevel.Information, "SMTP server started");
	}
}