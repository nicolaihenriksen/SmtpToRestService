namespace SmtpToRest.Rest;

internal interface IHttpClientFactory
{
    IHttpClient Create(string baseAddress);
}