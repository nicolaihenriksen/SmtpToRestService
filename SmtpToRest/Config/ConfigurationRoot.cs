using System.Collections.Generic;
using System.Net;

namespace SmtpToRest.Config;

internal class ConfigurationRoot
{
	public string? SmtpHost { get; set; }
	public int[]? SmtpPorts { get; set; }
	public string? ApiToken { get; set; }
	public string? Endpoint { get; set; }
	public string? HttpMethod { get; set; } = WebRequestMethods.Http.Get;
	public SmtpRelayConfiguration? SmtpRelay { get; set; }
	public List<ConfigurationMapping>? Mappings { get; set; }
}