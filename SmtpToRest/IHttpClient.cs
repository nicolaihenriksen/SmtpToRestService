using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest
{
    public interface IHttpClient
    {
        Uri BaseAddress { get; }
        IHttpRequestHeaders DefaultRequestHeaders { get; }
        Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content, CancellationToken cancellationToken);
        Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken cancellationToken);
    }
}