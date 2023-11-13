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
	private const string CategoryTokenToReplacement = "TokenToReplacement";

    [Theory]
    [Trait(CategoryKey, CategoryTokenToReplacement)]
    [InlineData("$(to){9}")]
    [InlineData("$(TO){9,13}")]
    public async Task ProcessMessages_ShouldReplaceToTokenWithIndexAndLengthSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.FirstToAddress).Returns("receiver@elsewhere.com");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://elsewhere.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenToReplacement)]
    [InlineData("$(TO){[else]}")]
    [InlineData("$(TO){[ver]+4}")]
    public async Task ProcessMessages_ShouldReplaceToTokenWithIndexOfAndOptionalOffsetSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.FirstToAddress).Returns("receiver@elsewhere.com");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://elsewhere.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenToReplacement)]
    [InlineData("$(TO){[else],13}")]
    [InlineData("$(TO){[ver]+4,13}")]
    public async Task ProcessMessages_ShouldReplaceToTokenWithIndexOfAndOptionalOffsetAndLengthSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.FirstToAddress).Returns("receiver@elsewhere.com");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://elsewhere.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenToReplacement)]
    [InlineData("$(TO){[else],[zz]-4}")]
    [InlineData("$(TO){[ver]+4,[zz]-4}")]
    public async Task ProcessMessages_ShouldReplaceToTokenWithIndexOfStartAndEndSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.FirstToAddress).Returns("receiver@elsewhere.com.uk.zz");
        HttpMessageHandler.Expect(HttpMethod.Get, "http://elsewhere.com/path").Respond(HttpStatusCode.OK);

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        HttpMessageHandler.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [Trait(CategoryKey, CategoryTokenToReplacement)]
    [InlineData("$(to){100,2}")]
    [InlineData("$(to){3,100}")]
    public async Task ProcessMessages_ShouldFailToReplaceToToken_WhenTokenIncorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.FirstToAddress).Returns("receiver@elsewhere.com");

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        Assert.NotNull(result);
        result.IsSuccess.Should().BeFalse();
    }
}