using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using SmtpToRest.Config;
using SmtpToRest.Processing;
using SmtpToRest.Services.Smtp;
using System.Net;
using System.Net.Http;
using Xunit;

namespace SmtpToRest.IntegrationTests.Services.Smtp;

public partial class SmtpServerBackgroundServiceTests
{
	private const string CategoryBasic = "Basic";

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public void ProcessMessages_ShouldReturnFailedResult_WhenMessageDoesNotHaveSenderAddress()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Mock<IMimeMessage> message = new();
		message
			.SetupGet(m => m.Address)
			.Returns((string)null!);
		MockedRequest request = HttpMessageHandler.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = SendMessage(message.Object);

		// Assert
		Assert.NotNull(result);
		result.IsSuccess.Should().BeFalse();
		HttpMessageHandler.GetMatchCount(request).Should().Be(0);
		result.Error.Should().Contain("No address found in message");
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public void ProcessMessages_ShouldReturnFailedResult_WhenNoMappingIsFound()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		Mock<IMimeMessage> message = new();
		message
			.SetupGet(m => m.Address)
			.Returns("sender@somewhere.com");
		MockedRequest request = HttpMessageHandler.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = SendMessage(message.Object);

		// Assert
		Assert.NotNull(result);
		result.IsSuccess.Should().BeFalse();
		HttpMessageHandler.GetMatchCount(request).Should().Be(0);
		result.Error.Should().Contain("No mapping found for");
	}

	[Fact]
	[Trait(CategoryKey, CategoryBasic)]
	public void ProcessMessages_ShouldSucceed_WhenMatchingMappingIsFound()
	{
		// Arrange
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler.Expect(HttpMethod.Get, Configuration.Endpoint)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = SendMessage(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}
}