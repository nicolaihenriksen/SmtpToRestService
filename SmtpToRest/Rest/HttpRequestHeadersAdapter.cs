using System.Net.Http.Headers;

namespace SmtpToRest.Rest;

public class HttpRequestHeadersAdapter : IHttpRequestHeaders
{
    public AuthenticationHeaderValue? Authorization
    {
        get => _adaptee.Authorization;
        set => _adaptee.Authorization = value;
    }

    private readonly HttpRequestHeaders _adaptee;

    public HttpRequestHeadersAdapter(HttpRequestHeaders adaptee)
    {
        _adaptee = adaptee;
    }
}