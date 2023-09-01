using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using SmtpToRest.Processing;
using SmtpToRest.Services.Smtp;
using System.Net;
using System.Net.Http;
using SmtpToRest.Config;
using Xunit;

namespace SmtpToRest.IntegrationTests.Services.Smtp;

public partial class SmtpServerBackgroundServiceTests
{
	private const string CategoryHttpGet = "HTTP GET";

	[Fact]
	[Trait(CategoryKey, CategoryHttpGet)]
	public void ProcessMessages_ShouldIncludeApiToken_WhenProvidedInConfiguration()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Configuration.ApiToken = "someToken";
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint)
			.With(r => r.Headers?.Authorization is { Scheme: "Bearer", Parameter: "someToken" })
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = SendMessage(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryHttpGet)]
	public void ProcessMessages_ShouldRespectOverriddenValues_WhenOverriddenInMapping()
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
		ProcessResult? result = SendMessage(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact] [Trait(CategoryKey, CategoryHttpGet)]
	public void ProcessMessages_ShouldIncludeQueryString_WhenProvidedInMapping()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			QueryString = "someKey=someValue&someOtherKey=someOtherValue"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint)
			.WithQueryString("someKey", "someValue")
			.WithQueryString("someOtherKey", "someOtherValue")
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = SendMessage(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryHttpGet)]
	public void ProcessMessages_ShouldAppendService_WhenProvidedInMapping()
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
		ProcessResult? result = SendMessage(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}
}