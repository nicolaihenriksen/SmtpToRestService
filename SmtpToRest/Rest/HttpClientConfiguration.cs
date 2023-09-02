namespace SmtpToRest.Rest;

internal class HttpClientConfiguration : IHttpClientConfiguration
{
	public string HttpClientName { get; }
	
	public HttpClientConfiguration(string httpClientName)
	{
		HttpClientName = httpClientName;
	}
}