﻿using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using SmtpToRest.Processing;
using SmtpToRest.Services.Smtp;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmtpToRest.Config;
using SmtpToRest.Rest.Decorators;
using Xunit;
using HttpMethod = System.Net.Http.HttpMethod;

namespace SmtpToRest.IntegrationTests.Services.Smtp;

public partial class SmtpServerHostedServiceTests
{
	private const string CategoryCustomInjection = "Custom DI Injections";

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseCustomEndpoint_WhenOverriddenByCustomDecorator()
	{
		// Arrange
		await StartHost(services =>
		{
			services.AddSingleton<IRestInputDecorator>(_ => new CustomRestInputDecorator(
				(restInput, _, _) =>
				{
					restInput.Endpoint = "http://customendpoint";
				}));
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			CustomEndpoint = "http://endpointfrommapping"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, "http://customendpoint")
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseCustomService_WhenOverriddenByCustomDecorator()
	{
		// Arrange
		await StartHost(services =>
		{
			services.AddSingleton<IRestInputDecorator>(_ => new CustomRestInputDecorator(
				(restInput, _, _) =>
				{
					restInput.Service = "customService";
				}));
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			Service = "serviceFromMapping"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, $"{Configuration.Endpoint}/customService")
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseCustomApiToken_WhenOverriddenByCustomDecorator()
	{
		// Arrange
		await StartHost(services =>
		{
			services.AddSingleton<IRestInputDecorator>(_ => new CustomRestInputDecorator(
				(restInput, _, _) =>
				{
					restInput.ApiToken = "customApiToken";
				}));
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			CustomApiToken = "apiTokenFromMapping"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint!)
			.With(r => r.Headers?.Authorization is { Scheme: "Bearer", Parameter: "customApiToken" })
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseCustomHttpMethod_WhenOverriddenByCustomDecorator()
	{
		// Arrange
		await StartHost(services =>
		{
			services.AddSingleton<IRestInputDecorator>(_ => new CustomRestInputDecorator(
				(restInput, _, _) =>
				{
					restInput.HttpMethod = Rest.HttpMethod.Put;
				}));
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			CustomHttpMethod = Rest.HttpMethod.Post.ToString()
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Put, Configuration.Endpoint!)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseCustomQueryString_WhenOverriddenByCustomDecorator()
	{
		// Arrange
		await StartHost(services =>
		{
			services.AddSingleton<IRestInputDecorator>(_ => new CustomRestInputDecorator(
				(restInput, _, _) =>
				{
					restInput.QueryString = "someCustomKey=someCustomValue&someOtherCustomKey=someOtherCustomValue";
				}));
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new()
		{
			QueryString = "someKey=someValue&someOtherKey=someOtherValue"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint!)
			.WithQueryString("someCustomKey", "someCustomValue")
			.WithQueryString("someOtherCustomKey", "someOtherCustomValue")
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseCustomContent_WhenOverriddenByCustomDecorator()
	{
		// Arrange
		await StartHost(services =>
		{
			services.AddSingleton<IRestInputDecorator>(_ => new CustomRestInputDecorator(
				(restInput, _, _) =>
				{
					restInput.Content = "customContent";
				}));
		});
		Configuration.HttpMethod = Rest.HttpMethod.Post.ToString();
		ConfigurationMapping mapping = new()
		{
			Content = "contentFromMapping"
		};
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Post, Configuration.Endpoint!)
			.WithContent("customContent")
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseHttpMessageHandler_WhenInjected()
	{
		// Arrange
		Options.UseBuiltInHttpClientFactory = true;
		await StartHost(services =>
		{
			services.AddTransient<CustomHttpMessageHandler>();
		}, httpConfig =>
		{
			httpConfig.AddHttpMessageHandler<CustomHttpMessageHandler>();
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		Assert.NotNull(result);
		result.IsSuccess.Should().BeFalse();
		result.Error.Should().Be("CustomHttpMessageHandler terminated the request");
		AssertLog(LogLevel.Debug, "CustomHttpMessageHandler short-circuiting the HTTP Client");
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseHttpMessageHandlerWithDefaultName_WhenInjectedWithNullName()
	{
		// Arrange
		Options.UseBuiltInHttpClientFactory = true;
		Options.HttpClientName = null;
		await StartHost(services =>
		{
			services.AddTransient<CustomHttpMessageHandler>();
		}, httpConfig =>
		{
			httpConfig.AddHttpMessageHandler<CustomHttpMessageHandler>();
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		Assert.NotNull(result);
		result.IsSuccess.Should().BeFalse();
		result.Error.Should().Be("CustomHttpMessageHandler terminated the request");
		AssertLog(LogLevel.Debug, "CustomHttpMessageHandler short-circuiting the HTTP Client");
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseHttpMessageHandlerWithCustomName_WhenInjectedWithCustomName()
	{
		// Arrange
		Options.UseBuiltInHttpClientFactory = true;
		Options.HttpClientName = "CustomHttpClient";
		await StartHost(services =>
		{
			services.AddTransient<CustomHttpMessageHandler>();
		}, httpConfig =>
		{
			httpConfig.AddHttpMessageHandler<CustomHttpMessageHandler>();
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		Assert.NotNull(result);
		result.IsSuccess.Should().BeFalse();
		result.Error.Should().Be("CustomHttpMessageHandler terminated the request");
		AssertLog(LogLevel.Debug, "CustomHttpMessageHandler short-circuiting the HTTP Client");
	}

    [Fact]
    [Trait(CategoryKey, CategoryCustomInjection)]
    public async Task ProcessMessages_ShouldUseHttpMessageHandlerWithCustomName_WhenOverriddenInMapping()
    {
        // Arrange
        Options.UseBuiltInHttpClientFactory = true;
        await StartHost(services =>
        {
            services.AddTransient<CustomHttpMessageHandler>();
            services.AddHttpClient("CustomHttpClient").AddHttpMessageHandler<CustomHttpMessageHandler>();
        });
        Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
        ConfigurationMapping mapping = new()
        {
			CustomHttpClientName = "CustomHttpClient"
        };
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);

        // Act
        ProcessResult ? result = await SendMessageAsync(message.Object);

        // Assert
        Assert.NotNull(result);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("CustomHttpMessageHandler terminated the request");
        AssertLog(LogLevel.Debug, "CustomHttpMessageHandler short-circuiting the HTTP Client");
    }

    [Fact]
    [Trait(CategoryKey, CategoryCustomInjection)]
    public async Task ProcessMessages_ShouldUseHttpMessageHandlerWithCustomName_WhenOverriddenInEmailContent()
    {
        // Arrange
        Options.UseBuiltInHttpClientFactory = true;
        await StartHost(services =>
        {
            services.AddTransient<CustomHttpMessageHandler>();
            services.AddHttpClient("CustomHttpClient").AddHttpMessageHandler<CustomHttpMessageHandler>();
        });
        Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
        ConfigurationMapping mapping = new()
        {
            CustomHttpClientName = "$(body)"
        };
        Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
        message.SetupGet(m => m.BodyAsString).Returns("CustomHttpClient");

        // Act
        ProcessResult? result = await SendMessageAsync(message.Object);

        // Assert
        Assert.NotNull(result);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("CustomHttpMessageHandler terminated the request");
        AssertLog(LogLevel.Debug, "CustomHttpMessageHandler short-circuiting the HTTP Client");
    }

    [Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseAdditionalMessageProcessor_WhenInjected()
	{
		// Arrange
		await StartHost(services =>
		{
			services.AddSingleton<IMessageProcessor, CustomMessageProcessor>();
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		ConfigurationMapping mapping = new();
		Mock<IMimeMessage> message = Arrange("sender@somewhere.com", mapping);
		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint!)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
		AssertLog(LogLevel.Debug, "CustomMessageProcessor pre-processing message");
	}

	[Fact]
	[Trait(CategoryKey, CategoryCustomInjection)]
	public async Task ProcessMessages_ShouldUseCustomMappingKeyExtractor_WhenInjected()
	{
		// Arrange
		await StartHost(services =>
		{
			services.AddSingleton<IConfigurationMappingKeyExtractor, CustomConfigurationMappingKeyExtractor>();
		});
		Configuration.HttpMethod = Rest.HttpMethod.Get.ToString();
		const string recipient = "recipient@somewhere.com";
		ConfigurationMapping mapping = new() { Key = recipient };
		Mock<IMimeMessage> message = new();
		message
			.SetupGet(m => m.FirstFromAddress)
			.Returns("sender@somewhere.com");
		message
			.SetupGet(m => m.FirstToAddress)
			.Returns(recipient);
		Configuration.AddMapping(recipient, mapping);

		HttpMessageHandler
			.Expect(HttpMethod.Get, Configuration.Endpoint!)
			.Respond(HttpStatusCode.OK);

		// Act
		ProcessResult? result = await SendMessageAsync(message.Object);

		// Assert
		HttpMessageHandler.VerifyNoOutstandingExpectation();
		Assert.NotNull(result);
		result.IsSuccess.Should().BeTrue();
	}
}