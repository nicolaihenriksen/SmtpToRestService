using System.Net.Http.Headers;

namespace SmtpToRest.Rest;

public interface IHttpRequestHeaders
{
    AuthenticationHeaderValue? Authorization { get; set; }
}