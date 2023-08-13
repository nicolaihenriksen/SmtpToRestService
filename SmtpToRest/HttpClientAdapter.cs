using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest;

public class HttpClientAdapter : IHttpClient
{
    public Uri? BaseAddress => _adaptee.BaseAddress;
    public IHttpRequestHeaders DefaultRequestHeaders { get; }

    private readonly HttpClient _adaptee;
    public HttpClientAdapter(HttpClient adaptee)
    {
        _adaptee = adaptee;
        DefaultRequestHeaders = new HttpRequestHeadersAdapter(adaptee.DefaultRequestHeaders);
    }
        
    public Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content, CancellationToken cancellationToken)
    {
        return _adaptee.PostAsync(uri, content, cancellationToken);
    }

    public Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken cancellationToken)
    {
        return _adaptee.GetAsync(uri, cancellationToken);
    }
}