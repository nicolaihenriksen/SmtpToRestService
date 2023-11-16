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
	private const string CategoryTokenFromReplacement = "TokenFromReplacement";

    [Theory]
    [Trait(CategoryKey, CategoryTokenFromReplacement)]
    [InlineData("$(from){7}")]
    [InlineData("$(FROM){7,13}")]
    public async Task ProcessMessages_ShouldReplaceFromTokenWithIndexAndLengthSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        HttpMessageHandler.Expect(HttpMethod.Get, "http://somewhere.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenFromReplacement)]
    [InlineData("$(FROM){[some]}")]
    [InlineData("$(FROM){[der]+4}")]
    public async Task ProcessMessages_ShouldReplaceFromTokenWithIndexOfAndOptionalOffsetSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        HttpMessageHandler.Expect(HttpMethod.Get, "http://somewhere.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenFromReplacement)]
    [InlineData("$(FROM){[some],13}")]
    [InlineData("$(FROM){[der]+4,13}")]
    public async Task ProcessMessages_ShouldReplaceFromTokenWithIndexOfAndOptionalOffsetAndLengthSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        HttpMessageHandler.Expect(HttpMethod.Get, "http://somewhere.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenFromReplacement)]
    [InlineData("$(FROM){[some],[zz]-4}")]
    [InlineData("$(FROM){[der]+4,[zz]-4}")]
    public async Task ProcessMessages_ShouldReplaceFromTokenWithIndexOfStartAndEndSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com.uk.zz", mapping);
        HttpMessageHandler.Expect(HttpMethod.Get, "http://somewhere.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }
}