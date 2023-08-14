namespace SmtpToRest.Rest;

public interface IHttpClientFactory
{
    IHttpClient Create(string baseAddress);
}