using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using SmtpToRest.Config;
using Xunit;

namespace SmtpToRest.UnitTests.Config;

public class ConfigurationTests
{
    [Fact]
    public void Ctor_ShouldThrowException_WhenInvalidJsonInConfigurationFile()
    {
        // Arrange
        var log = new Mock<ILogger<Configuration>>();
        var configProvider = new Mock<IConfigurationProvider>();
        configProvider.Setup(c => c.GetConfigurationFileDirectory()).Returns(string.Empty);
        var configReader = new Mock<IConfigurationFileReader>();
        configReader.Setup(c => c.Read(It.IsAny<string>())).Returns("{ invalid JSON document }");

        // Act
        Action act = () => { Configuration _ = new(log.Object, configProvider.Object, configReader.Object, false); };

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Ctor_ShouldCorrectlyReadGeneralSettings()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            endpoint = "<endpoint>",
            apiToken = "<token>",
            httpMethod = "<httpMethod>"
        });

        var log = new Mock<ILogger<Configuration>>();
        var configProvider = new Mock<IConfigurationProvider>();
        configProvider.Setup(c => c.GetConfigurationFileDirectory()).Returns(string.Empty);
        var configReader = new Mock<IConfigurationFileReader>();
        configReader.Setup(c => c.Read(It.IsAny<string>())).Returns(json);

        // Act
        var config = new Configuration(log.Object, configProvider.Object, configReader.Object, false);

        // Assert
        config.Endpoint.Should().Be("<endpoint>");
        config.ApiToken.Should().Be("<token>");
        config.HttpMethod.Should().Be("<httpMethod>");
    }

    [Fact]
    public void Ctor_ShouldCorrectlyReadMappings()
    {
        // Arrange
        var json = JsonSerializer.Serialize(new
        {
            mappings = new List<dynamic>
            {
                new
                {
                    key = "<key1>",
                    customApiToken = "<token1>",
                    customEndpoint = "<endpoint1>",
                    customHttpMethod = "<httpMethod1>",
                    service = "<service1>",
                    queryString = "<queryString1>",
                    content = "<contentData1>"
                },
                new
                {
                    key = "<key2>",
                    customApiToken = "<token2>",
                    customEndpoint = "<endpoint2>",
                    customHttpMethod = "<httpMethod2>",
                    service = "<service2>",
                    queryString = "<queryString2>",
                    content = "<contentData2>"
                }
            }
        });

        var log = new Mock<ILogger<Configuration>>();
        var configProvider = new Mock<IConfigurationProvider>();
        configProvider.Setup(c => c.GetConfigurationFileDirectory()).Returns(string.Empty);
        var configReader = new Mock<IConfigurationFileReader>();
        configReader.Setup(c => c.Read(It.IsAny<string>())).Returns(json);

        // Act
        var config = new Configuration(log.Object, configProvider.Object, configReader.Object, false);

        // Assert
        if (!config.TryGetMapping("<key1>", out var mapping1) || mapping1 is null)
            throw new AssertionFailedException("Unable to read mapping");
        if (!config.TryGetMapping("<key2>", out var mapping2) || mapping2 is null)
            throw new AssertionFailedException("Unable to read mapping");

        mapping1.CustomApiToken.Should().Be("<token1>");
        mapping1.CustomEndpoint.Should().Be("<endpoint1>");
        mapping1.CustomHttpMethod.Should().Be("<httpMethod1>");
        mapping1.Service.Should().Be("<service1>");
        mapping1.QueryString.Should().Be("<queryString1>");
        Assert.Equal("<contentData1>", mapping1.Content?.ToString());
        mapping2.CustomApiToken.Should().Be("<token2>");
        mapping2.CustomEndpoint.Should().Be("<endpoint2>");
        mapping2.CustomHttpMethod.Should().Be("<httpMethod2>");
        mapping2.Service.Should().Be("<service2>");
        mapping2.QueryString.Should().Be("<queryString2>");
        Assert.Equal("<contentData2>", mapping2.Content?.ToString());
    }

    [Fact]
    public void Ctor_ShouldCorrectlyReadMapping_WhenJsonPostDataIsJsonObject()
    {
        // Arrange
        var contentObject = new
        {
            customSimpleType = 12,
            customComplexType = new
            {
                stringField = "stringValue",
                doubleField = 1.234
            }
        };
        var json = JsonSerializer.Serialize(new
        {
            mappings = new List<dynamic>
            {
                new
                {
                    key = "<key>",
                    content = contentObject
                },
            }
        });
        var log = new Mock<ILogger<Configuration>>();
        var configProvider = new Mock<IConfigurationProvider>();
        configProvider.Setup(c => c.GetConfigurationFileDirectory()).Returns(string.Empty);
        var configReader = new Mock<IConfigurationFileReader>();
        configReader.Setup(c => c.Read(It.IsAny<string>())).Returns(json);

        // Act
        var config = new Configuration(log.Object, configProvider.Object, configReader.Object, false);

        // Assert
        if (!config.TryGetMapping("<key>", out var mapping) || mapping is null)
            throw new AssertionFailedException("Unable to read mapping");
        var jsonPostDataString = JsonSerializer.Serialize(contentObject);
        Assert.Equal(jsonPostDataString, mapping.Content?.ToString());
    }
}