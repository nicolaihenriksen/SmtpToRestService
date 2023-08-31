using System.Collections.Generic;
using System.Net;
using SmtpToRest.Config;

namespace SmtpToRest.IntegrationTests;

internal class TestConfiguration : IConfiguration
{
	public string? SmtpHost { get; } = "localhost";
	public int[]? SmtpPorts { get; } = new[] {25};
	public string? ApiToken { get; } = "TestApiToken";
	public string? Endpoint { get; } = "http://testendpoint";
	public string? HttpMethod { get; } = WebRequestMethods.Http.Get;

	private readonly Dictionary<string, ConfigurationMapping> _mappings = new();

	public void AddMapping(string key, ConfigurationMapping mapping)
	{
		_mappings.Add(key, mapping);
	}

	public bool TryGetMapping(string key, out ConfigurationMapping? mapping)
	{
		return _mappings.TryGetValue(key, out mapping);
	}
}