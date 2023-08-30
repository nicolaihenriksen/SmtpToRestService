using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using RichardSzalay.MockHttp;
using SmtpToRest.Rest;
using Xunit;
using HttpMethod = System.Net.Http.HttpMethod;

namespace SmtpToRest.UnitTests;

public class RestClientTests
{
	private AutoMocker AutoMocker { get; } = new();
	private MockHttpMessageHandler HttpMessageHandler { get; } = new();

	public RestClientTests()
	{
		AutoMocker.GetMock<IHttpClientFactory>()
			.Setup(f => f.CreateClient(It.IsAny<string>()))
			.Returns(new HttpClient(HttpMessageHandler));
	}

	[Fact]
	public async Task InvokeService_Should_When()
	{
		// Arrange
		IRestClient client = AutoMocker.CreateInstance<RestClient>();
		HttpMessageHandler.Expect(HttpMethod.Get, "http://localhost").Respond(HttpStatusCode.OK);
		RestInput input = new RestInput { Endpoint = "http://localhost" };

		// Act
		HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		HttpMessageHandler.VerifyNoOutstandingExpectation();
	}
}