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
	private const string CategoryHttpPost = "HTTP POST";

	[Fact]
	[Trait(CategoryKey, CategoryHttpPost)]
	public async Task ProcessMessages_ShouldIncludeJsonContent_WhenSuppliedInMapping()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Post.ToString();
		ConfigurationMapping mapping = new()
		{
			Content = new
			{
				Id = 1,
				Username = "test",
			}
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Post, Configuration.Endpoint!)
			.WithContent("{\"Id\":1,\"Username\":\"test\"}")
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryHttpPost)]
	public async Task ProcessMessages_ShouldIncludeStringContent_WhenSuppliedInMapping()
	{
		// Arrange
		Configuration.HttpMethod = Rest.HttpMethod.Post.ToString();
		ConfigurationMapping mapping = new()
		{
			Content = "SomeStringValue"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Post, Configuration.Endpoint!)
			.WithContent("SomeStringValue")
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}
}