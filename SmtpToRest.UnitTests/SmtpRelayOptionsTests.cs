using System.Text.Json;
using FluentAssertions;
using GenFu;
using SmtpToRest.Config;
using Xunit;

namespace SmtpToRest.UnitTests;

public class SmtpRelayOptionsTests
{
	[Fact]
	public void Ctor_ShouldShouldCopyAllProperties_WhenOriginalPassedAsArgument()
	{
		// Arrange
		SmtpRelayOptions original = A.New<SmtpRelayOptions>();

		// Act
		SmtpRelayOptions options = new(original);

		// Assert
		JsonSerializer.Serialize(options).Should().Be(JsonSerializer.Serialize(original));
	}

	[Fact]
	public void Override_ShouldGracefullyHandle_WhenConfigurationIsNull()
	{
		// Arrange
		SmtpRelayOptions options = A.New<SmtpRelayOptions>();
		string expected = JsonSerializer.Serialize(options);
		SmtpRelayConfiguration? configuration = null;

		// Act
		options.Override(configuration);

		// Assert
		JsonSerializer.Serialize(options).Should().Be(expected);
	}

	[Fact]
	public void Override_ShouldOverrideEnabled_WhenConfigurationOverridesEnabled()
	{
		// Arrange
		SmtpRelayOptions options = A.New<SmtpRelayOptions>();
		SmtpRelayConfiguration configuration = new() { Enabled = !options.Enabled };

		// Act
		options.Override(configuration);

		// Assert
		options.Enabled.Should().Be(configuration.Enabled.Value);
	}

	[Fact]
	public void Override_ShouldOverrideAuthenticate_WhenConfigurationOverridesAuthenticate()
	{
		// Arrange
		SmtpRelayOptions options = A.New<SmtpRelayOptions>();
		SmtpRelayConfiguration configuration = new() { Authenticate = !options.Authenticate };

		// Act
		options.Override(configuration);

		// Assert
		options.Authenticate.Should().Be(configuration.Authenticate.Value);
	}
}