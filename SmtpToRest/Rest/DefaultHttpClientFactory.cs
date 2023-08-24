using System;
using System.Net.Http;

namespace SmtpToRest.Rest;

internal class DefaultHttpClientFactory : IHttpClientFactory
{
    public IHttpClient Create(string baseAddress)
    {
        var adaptee = new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        };
        return new HttpClientAdapter(adaptee);
    }
}