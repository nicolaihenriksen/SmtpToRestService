namespace SmtpToRest
{
    public interface IHttpClientFactory
    {
        IHttpClient Create(string baseAddress);
    }
}