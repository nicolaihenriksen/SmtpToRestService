namespace SmtpToRestService
{
    internal interface IHttpClientFactory
    {
        IHttpClient Create(string baseAddress);
    }
}