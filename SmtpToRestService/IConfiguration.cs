namespace SmtpToRestService
{
    internal interface IConfiguration
    {
        string ApiToken { get; }
        string Endpoint { get; }
        string HttpMethod { get; }
        bool TryGetMapping(string key, out ConfigurationMapping mapping);
    }
}