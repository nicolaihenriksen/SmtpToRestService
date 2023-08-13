using System.Net.Http.Headers;

namespace SmtpToRest;

public interface IHttpRequestHeaders
{
    AuthenticationHeaderValue Authorization { get; set; }
}