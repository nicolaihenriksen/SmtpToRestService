﻿using FluentAssertions;
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
	private const string CategoryTokenReplacement = "TokenReplacement";

	[Theory]
	[Trait(CategoryKey, CategoryTokenReplacement)]
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
    [Trait(CategoryKey, CategoryTokenReplacement)]
    [InlineData("$(body){25,16}")]
    [InlineData("$(BODY){25,16}")]
    [InlineData("$(body){25}")]
    [InlineData("$(BODY){25}")]
    public async Task ProcessMessages_ShouldReplaceBodyTokenWithIndexAndLengthSubset_WhenTokenCorrectlyAppliedInEndpoint(string token)
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
    [Trait(CategoryKey, CategoryTokenReplacement)]
    [InlineData("$(body){100,2}")]
    [InlineData("$(body){3,100}")]
    [InlineData("$(body){3}")]
    public async Task ProcessMessages_ShouldFailToReplaceBodyToken_WhenTokenIncorrectlyAppliedInEndpoint(string token)
    {
        // Arrange
        Configuration.Endpoint = $"http://{token}/path";
        ConfigurationMapping mapping = new();
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("At token-domain.com, you can find cool stuff!");

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        Assert.NotNull(result);
        result.IsSuccess.Should().BeFalse();
    }
}