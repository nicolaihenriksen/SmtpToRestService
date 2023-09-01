using System;
using System.Collections.Generic;
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
using InputHttpMethod = SmtpToRest.Rest.HttpMethod;
using HttpMethod = System.Net.Http.HttpMethod;

namespace SmtpToRest.UnitTests.Rest;

public class RestClientTests
{
    private const string BaseAddress = "http://localhost";
    private AutoMocker AutoMocker { get; } = new();
    private MockHttpMessageHandler HttpMessageHandler { get; } = new();

    public RestClientTests()
    {
        AutoMocker.GetMock<IHttpClientFactory>()
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(new HttpClient(HttpMessageHandler));
    }

    [Fact]
    public async Task InvokeService_ShouldReturnNotFound_WhenNoEndpointIsSupplied()
    {
        // Arrange
        IRestClient client = AutoMocker.CreateInstance<RestClient>();
        RestInput input = new RestInput { Endpoint = null };

        // Act
        HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeService_ShouldApplyBearerToken_WhenApiKeyIsSupplied()
    {
        // Arrange
        const string apiKey = "someApiKey";
        IRestClient client = AutoMocker.CreateInstance<RestClient>();
        HttpMessageHandler.Expect(HttpMethod.Get, BaseAddress)
            .With(r => r.Headers?.Authorization is { Scheme: "Bearer", Parameter: apiKey })
            .Respond(HttpStatusCode.OK);
        RestInput input = new RestInput { Endpoint = BaseAddress, ApiToken = apiKey };

        // Act
        HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task InvokeService_ShouldApplyServiceToBaseAddress_WhenServiceIsSupplied()
    {
        // Arrange
        const string requestedService = "someService";
        IRestClient client = AutoMocker.CreateInstance<RestClient>();
        HttpMessageHandler.Expect(HttpMethod.Get, $"{BaseAddress}/{requestedService}").Respond(HttpStatusCode.OK);
        RestInput input = new RestInput { Endpoint = BaseAddress, Service = requestedService };

        // Act
        HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData(InputHttpMethod.Post)]
    [InlineData(InputHttpMethod.Get)]
    [InlineData(InputHttpMethod.Connect)]
    [InlineData(InputHttpMethod.Delete)]
    [InlineData(InputHttpMethod.Head)]
    [InlineData(InputHttpMethod.Options)]
    [InlineData(InputHttpMethod.Patch)]
    [InlineData(InputHttpMethod.Put)]
    [InlineData(InputHttpMethod.Trace)]
    public async Task InvokeService_ShouldRespectHttpMethodFromInput(InputHttpMethod inputMethod)
    {
        // Arrange
        Dictionary<InputHttpMethod, HttpMethod> expectations = new()
        {
            { InputHttpMethod.Post, HttpMethod.Post },
            { InputHttpMethod.Get, HttpMethod.Get },
            { InputHttpMethod.Connect, HttpMethod.Connect },
            { InputHttpMethod.Delete, HttpMethod.Delete },
            { InputHttpMethod.Head, HttpMethod.Head },
            { InputHttpMethod.Options, HttpMethod.Options },
            { InputHttpMethod.Patch, HttpMethod.Patch },
            { InputHttpMethod.Put, HttpMethod.Put },
            { InputHttpMethod.Trace, HttpMethod.Trace },
        };
        HttpMethod expectedMethod = expectations[inputMethod];
        IRestClient client = AutoMocker.CreateInstance<RestClient>();
        HttpMessageHandler.Expect(expectedMethod, BaseAddress).Respond(HttpStatusCode.OK);
        RestInput input = new RestInput { Endpoint = BaseAddress, HttpMethod = inputMethod };

        // Act
        HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task InvokeService_ShouldIncludeQueryString_WhenMethodIsGetAndQueryStringIsSupplied()
    {
        // Arrange
        const string queryString = "value1=1&value2=2&value3=3";
        IRestClient client = AutoMocker.CreateInstance<RestClient>();
        HttpMessageHandler.Expect(HttpMethod.Get, BaseAddress)
            .WithQueryString("value1", "1")
            .WithQueryString("value2", "2")
            .WithQueryString("value3", "3")
			.Respond(HttpStatusCode.OK);
        RestInput input = new RestInput { Endpoint = BaseAddress, QueryString = queryString };

        // Act
        HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task InvokeService_ShouldEscapeQueryString_WhenMethodIsGetAndQueryStringIsSupplied()
    {
        // Arrange
        const string queryString = "stringValue=This is a text that needs escaping because 1 < 2";
        string escaped = Uri.EscapeDataString(queryString);
        IRestClient client = AutoMocker.CreateInstance<RestClient>();
        HttpMessageHandler.Expect(HttpMethod.Get, BaseAddress)
            .WithQueryString("stringValue", "This is a text that needs escaping because 1 < 2")
            .Respond(HttpStatusCode.OK);
        RestInput input = new RestInput { Endpoint = BaseAddress, QueryString = queryString };

        // Act
        HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

        // Assert
        escaped.Should().NotContain(" ").And.NotContain("<");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task InvokeService_ShouldIncludeJsonPostData_WhenMethodIsPostAndJsonPostDataIsSupplied()
    {
		// Arrange
		const string postData = """{ "RootProperty": "RootPropertyValue, "ChildObject": { "ChildProperty": "ChildPropertyValue"}}""";
        IRestClient client = AutoMocker.CreateInstance<RestClient>();
        HttpMessageHandler.Expect(HttpMethod.Post, BaseAddress)
            .WithContent(postData)
            .Respond(HttpStatusCode.OK);
        RestInput input = new RestInput { Endpoint = BaseAddress, HttpMethod = InputHttpMethod.Post, Content = postData };

        // Act
        HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HttpMessageHandler.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task InvokeService_ShouldNotIncludeJsonPostData_WhenMethodIsNotPostAndJsonPostDataIsSupplied()
    {
        // Arrange
        const string postData = """{ "RootProperty": "RootPropertyValue, "ChildObject": { "ChildProperty": "ChildPropertyValue"}}""";
        IRestClient client = AutoMocker.CreateInstance<RestClient>();
        HttpMessageHandler.Expect(HttpMethod.Get, BaseAddress)
            .Respond(r => new HttpResponseMessage(r.Content is null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError));
        RestInput input = new RestInput { Endpoint = BaseAddress, HttpMethod = InputHttpMethod.Get, Content = postData };

        // Act
        HttpResponseMessage response = await client.InvokeService(input, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HttpMessageHandler.VerifyNoOutstandingExpectation();
    }
}