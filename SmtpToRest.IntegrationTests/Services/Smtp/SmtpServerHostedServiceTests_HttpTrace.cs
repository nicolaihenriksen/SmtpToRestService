using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using SmtpToRest.Processing;
using SmtpToRest.Services.Smtp;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SmtpToRest.Config;
using Xunit;

namespace SmtpToRest.IntegrationTests.Services.Smtp;

public partial class SmtpServerHostedServiceTests
{
	private const string CategoryHttpTrace = "HTTP TRACE";

	[Fact]
	[Trait(CategoryKey, CategoryHttpTrace)]
	public async Task ProcessMessages_ShouldSendTraceRequest_WhenSuppliedInMapping()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Trace.ToString();
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Trace, Configuration.Endpoint!)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}
}
