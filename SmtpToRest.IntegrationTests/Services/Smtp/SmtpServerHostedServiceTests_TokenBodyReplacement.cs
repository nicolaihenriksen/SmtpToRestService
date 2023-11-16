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
	private const string CategoryTokenBodyReplacement = "TokenBodyReplacement";

	[Theory]
	[Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(body)")]
    [InlineData("$(BODY)")]
    public async Task ProcessMessages_ShouldReplaceBodyToken_WhenTokenAppliedInEndpoint(string token)
	{
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("token-domain.com");
		HttpMessageHandler.Expect(HttpMethod.Get, "http://token-domain.com/path").Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(body)")]
    [InlineData("$(BODY)")]
    public async Task ProcessMessages_ShouldReplaceBodyToken_WhenTokenAppliedInQueryString(string token)
    {
        // Arrange
        ConfigurationMapping mapping = new()
        {
            QueryString = $"p={token}"
        };
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("bodyValue");
        HttpMessageHandler.Expect(HttpMethod.Get, string.Concat(Configuration.Endpoint!, "?p=bodyValue")).Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(body){25,16}")]
    [InlineData("$(BODY){25,16}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexAndLengthSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("Go see something cool at token-domain.com you'll like it");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://token-domain.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(body){21,9}")]
    [InlineData("$(BODY){21,9}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexAndLengthSubset_WhenTokenCorrectlyAppliedInQueryString(string token)
    {
        // Arrange
        ConfigurationMapping mapping = new()
        {
            QueryString = $"p={token}"
        };
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("A message containing bodyValue at some point in the string");
        HttpMessageHandler.Expect(HttpMethod.Get, string.Concat(Configuration.Endpoint!, "?p=bodyValue")).Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(body){25}")]
    [InlineData("$(BODY){25}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("Go see something cool at token-domain.com");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://token-domain.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(body){21}")]
    [InlineData("$(BODY){21}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexSubset_WhenTokenCorrectlyAppliedInQueryString(string token)
    {
        // Arrange
        ConfigurationMapping mapping = new()
        {
            QueryString = $"p={token}"
        };
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("A message containing bodyValue");
        HttpMessageHandler.Expect(HttpMethod.Get, string.Concat(Configuration.Endpoint!, "?p=bodyValue")).Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(BODY){[token-domain]}")]
    [InlineData("$(BODY){[at]+3}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexOfAndOptionalOffsetSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("Go see something cool at token-domain.com");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://token-domain.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(BODY){[body]}")]
    [InlineData("$(BODY){[containing]+11}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexOfAndOptionalOffsetSubset_WhenTokenCorrectlyAppliedInQueryString(string token)
    {
        // Arrange
        ConfigurationMapping mapping = new()
        {
            QueryString = $"p={token}"
        };
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("A message containing bodyValue");
        HttpMessageHandler.Expect(HttpMethod.Get, string.Concat(Configuration.Endpoint!, "?p=bodyValue")).Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(BODY){[token-domain],16}")]
    [InlineData("$(BODY){[at]+3,16}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexOfAndOptionalOffsetAndLengthSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("Go see something cool at token-domain.com you'll like it");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://token-domain.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(BODY){[body],9}")]
    [InlineData("$(BODY){[containing]+11,9}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexOfAndOptionalOffsetAndLengthSubset_WhenTokenCorrectlyAppliedInQueryString(string token)
    {
        // Arrange
        ConfigurationMapping mapping = new()
        {
            QueryString = $"p={token}"
        };
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("A message containing bodyValue at some point in the string");
        HttpMessageHandler.Expect(HttpMethod.Get, string.Concat(Configuration.Endpoint!, "?p=bodyValue")).Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(BODY){[token-domain],[like]-8}")]
    [InlineData("$(BODY){[at]+3,[like]-8}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexOfStartAndEndSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("Go see something cool at token-domain.com you'll like it");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://token-domain.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenBodyReplacement)]
    [InlineData("$(BODY){[body],[some]-4}")]
    [InlineData("$(BODY){[containing]+11,[point]-9}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexOfStartAndEndSubset_WhenTokenCorrectlyAppliedInQueryString(string token)
    {
        // Arrange
        ConfigurationMapping mapping = new()
        {
            QueryString = $"p={token}"
        };
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("A message containing bodyValue at some point in the string");
        HttpMessageHandler.Expect(HttpMethod.Get, string.Concat(Configuration.Endpoint!, "?p=bodyValue")).Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }
}