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
	[Fact]
	public void ProcessMessages_Should_When()
	{
		// Arrange
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", new ConfigurationMapping());
		HttpMessageHandler.Expect(HttpMethod.Get, "http://testendpoint").Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = SendMessage(message.Object);

		// Assert
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
		HttpMessageHandler.VerifyNoOutstandingExpectation();
	}
}