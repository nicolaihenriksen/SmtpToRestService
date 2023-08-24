using System.Collections.Generic;
using System.Net;

namespace SmtpToRest.Config;

internal class ConfigurationRoot
{
	public string? ApiToken { get; set; }
	public string? Endpoint { get; set; }
	public string? HttpMethod { get; set; } = WebRequestMethods.Http.Get;
	public List<ConfigurationMapping>? Mappings { get; set; }
}