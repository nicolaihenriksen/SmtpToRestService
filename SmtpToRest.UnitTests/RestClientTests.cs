using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SmtpToRest;
using Xunit;

namespace SmtpToRestService.UnitTests
{
    public class RestClientTests
    {
        private const string DefaultBaseAddress = "http://somerestapi.com/";

        private static IConfiguration Arrange_CreateConfiguration(string baseAddress = DefaultBaseAddress, string apiToken = null, string httpMethod = null)
        {
            var config = new Mock<IConfiguration>();
            config.SetupGet(c => c.Endpoint).Returns(baseAddress);
            if (!string.IsNullOrEmpty(apiToken))
                config.SetupGet(c => c.ApiToken).Returns(apiToken);
            if (!string.IsNullOrEmpty(httpMethod))
                config.SetupGet(c => c.HttpMethod).Returns(httpMethod);
            return config.Object;
        }

        private static (Mock<IHttpClient> client, Mock<IHttpRequestHeaders> headers) Arrange_CreateMockHttpClient(string baseAddress)
        {
            var client = new Mock<IHttpClient>();
            var requestHeaders = new Mock<IHttpRequestHeaders>();
            client.SetupGet(c => c.BaseAddress).Returns(new Uri(baseAddress));
            client.SetupGet(c => c.DefaultRequestHeaders).Returns(requestHeaders.Object);
            return (client, requestHeaders);
        }
        
        private static IHttpClientFactory Arrange_CreateHttpClientFactory(Mock<IHttpClient> client, string optionalBaseAddress = null)
        {
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.Create(client.Object.BaseAddress.AbsoluteUri)).Returns(client.Object);
            if (!string.IsNullOrEmpty(optionalBaseAddress))
                factory.Setup(f => f.Create(optionalBaseAddress)).Returns(client.Object).Callback(() =>
                {
                    client.SetupGet(c => c.BaseAddress).Returns(new Uri(optionalBaseAddress));
                });
            return factory.Object;
        }

        [Fact]
        public async Task InvokeService_ShouldSetApiToken_WhenTokenSetInDefaults()
        {
            // Arrange
            var config = Arrange_CreateConfiguration(apiToken: "<api token>");
            var (client, headers) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client);
            var mapping = new ConfigurationMapping { Service = "context" };
            var restClient = new RestClient(config, factory);

            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            headers.VerifySet(h => h.Authorization = It.Is<AuthenticationHeaderValue>(v => v.Scheme == "Bearer" && v.Parameter == "<api token>"), Times.Once);
        }

        [Fact]
        public async Task InvokeService_ShouldUseGetMethod_WhenGetSetInDefaults()
        {
            // Arrange
            var config = Arrange_CreateConfiguration(httpMethod: "GET");
            var (client, headers) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client);
            var mapping = new ConfigurationMapping { Service = "context" };
            var restClient = new RestClient(config, factory);

            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            var expectedUri = new Uri(DefaultBaseAddress + "context");
            client.Verify(c => c.GetAsync(expectedUri, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task InvokeService_ShouldUsePostMethod_WhenGetSetInDefaults()
        {
            // Arrange
            var config = Arrange_CreateConfiguration(httpMethod: "POST");
            var (client, headers) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client);
            var mapping = new ConfigurationMapping { Service = "context" };
            var restClient = new RestClient(config, factory);

            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            var expectedUri = new Uri(DefaultBaseAddress + "context");
            client.Verify(c => c.PostAsync(expectedUri, It.IsAny<HttpContent>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task InvokeService_ShouldUseGetMethod_WhenGetSetInMapping()
        {
            // Arrange
            var config = Arrange_CreateConfiguration(httpMethod: "POST");
            var (client, headers) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client);
            var mapping = new ConfigurationMapping { Service = "context", CustomHttpMethod = "GET" };
            var restClient = new RestClient(config, factory);

            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            var expectedUri = new Uri(DefaultBaseAddress + "context");
            client.Verify(c => c.GetAsync(expectedUri, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task InvokeService_ShouldUseCustomEndpoint_WhenEndpointSetInMapping()
        {
            // Arrange
            var customEndpoint = "http://customendpoint.com/";
            var config = Arrange_CreateConfiguration("http://defaultendpoint.com/");
            var (client, headers) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client, customEndpoint);
            var mapping = new ConfigurationMapping { Service = "context", CustomEndpoint = customEndpoint };
            var restClient = new RestClient(config, factory);

            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            var expectedUri = new Uri("http://customendpoint.com/" + "context");
            client.Verify(c => c.GetAsync(expectedUri, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task InvokeService_ShouldUsePostMethod_WhenGetSetInMapping()
        {
            // Arrange
            var config = Arrange_CreateConfiguration(httpMethod: "GET");
            var (client, headers) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client);
            var mapping = new ConfigurationMapping { Service = "context", CustomHttpMethod = "POST" };
            var restClient = new RestClient(config, factory);

            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            var expectedUri = new Uri(DefaultBaseAddress + "context");
            client.Verify(c => c.PostAsync(expectedUri, It.IsAny<HttpContent>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task InvokeService_ShouldUseDefaults_WhenUsingNoOverridesInMapping()
        {
            // Arrange
            var config = Arrange_CreateConfiguration();
            var (client, _) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client);
            var mapping = new ConfigurationMapping { Service = "context" };
            var restClient = new RestClient(config, factory);

            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            var expectedUri = new Uri(DefaultBaseAddress + "context");
            client.Verify(c => c.GetAsync(expectedUri, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task InvokeService_ShouldUseQueryString_WhenSetInMapping()
        {
            // Arrange
            var config = Arrange_CreateConfiguration();
            var (client, _) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client);
            var mapping = new ConfigurationMapping { Service = "context", QueryString = "param1=value1&param2=value2"};
            var restClient = new RestClient(config, factory);

            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            var expectedUri = new Uri(DefaultBaseAddress + "context?param1=value1&param2=value2");
            client.Verify(c => c.GetAsync(expectedUri, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task InvokeService_ShouldUseJsonPostData_WhenSetInMapping()
        {
            // Arrange
            var jsonPostData = "{ \"param1\": \"value1\", \"param2\": \"value2\" }";
            var json = JsonSerializer.Deserialize<dynamic>(jsonPostData);
            var config = Arrange_CreateConfiguration(httpMethod: "POST");
            var (client, _) = Arrange_CreateMockHttpClient(config.Endpoint);
            var factory = Arrange_CreateHttpClientFactory(client);
            var mapping = new ConfigurationMapping { Service = "context", JsonPostData = json };
            var restClient = new RestClient(config, factory);
            
            // Act
            await restClient.InvokeService(mapping, CancellationToken.None);

            // Assert
            var expectedUri = new Uri(DefaultBaseAddress + "context");
            client.Verify(c => c.PostAsync(expectedUri, It.Is<StringContent>(sc => sc.ReadAsStringAsync().Result == jsonPostData), CancellationToken.None), Times.Once);
        }
    }
}