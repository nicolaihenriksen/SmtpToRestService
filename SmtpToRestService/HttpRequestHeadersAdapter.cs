using System.Net.Http.Headers;

namespace SmtpToRestService
{
    internal class HttpRequestHeadersAdapter : IHttpRequestHeaders
    {
        public AuthenticationHeaderValue Authorization
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
}