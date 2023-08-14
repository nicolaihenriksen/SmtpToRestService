namespace SmtpToRest.Config;

public class ConfigurationMapping
{
	public string? Key { get; set; }
	public string? CustomApiToken { get; set; }
	public string? CustomEndpoint { get; set; }
	public string? CustomHttpMethod { get; set; }
	public string? Service { get; set; }
	public string? QueryString { get; set; }
	public dynamic? JsonPostData { get; set; }
}