using System.Collections.Generic;
using System.Net;
using SmtpToRest.Config;

namespace SmtpToRest.IntegrationTests;

internal class TestConfiguration : IConfiguration
{
	public string? SmtpHost { get; set; } = "localhost";
	public int[]? SmtpPorts { get; set; } = new[] {25};
	public string? ApiToken { get; set; } = "TestApiToken";
	public string? Endpoint { get; set; } = "http://testendpoint";
	public string? HttpMethod { get; set; } = WebRequestMethods.Http.Get;

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