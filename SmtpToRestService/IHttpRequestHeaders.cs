using System.Net.Http.Headers;

namespace SmtpToRestService
{
    internal interface IHttpRequestHeaders
    {
        AuthenticationHeaderValue Authorization { get; set; }
    }
}