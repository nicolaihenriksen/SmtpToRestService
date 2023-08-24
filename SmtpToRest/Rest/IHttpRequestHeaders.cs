using System.Net.Http.Headers;

namespace SmtpToRest.Rest;

internal interface IHttpRequestHeaders
{
    AuthenticationHeaderValue? Authorization { get; set; }
}