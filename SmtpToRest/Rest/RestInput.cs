namespace SmtpToRest.Rest;

public class RestInput
{
	public string? ApiToken { get; set; }
	public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;
	public string? Endpoint { get; set; }
	public string? Service { get; set; }
	public string? QueryString { get; set; }
	public dynamic? JsonPostData { get; set; }
}