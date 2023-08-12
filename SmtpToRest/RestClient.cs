using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest
{
    public class RestClient : IRestClient
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public RestClient(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task<HttpResponseMessage> InvokeService(ConfigurationMapping mapping, CancellationToken? cancellationToken)
        {
            var endpoint = mapping.CustomEndpoint ?? _configuration.Endpoint;
            var apiToken = mapping.CustomApiToken ?? _configuration.ApiToken;
            var httpMethod = mapping.CustomHttpMethod ?? _configuration.HttpMethod;
            var isPost = WebRequestMethods.Http.Post == httpMethod;

            var client = _httpClientFactory.Create(endpoint);
            
            if (!string.IsNullOrEmpty(apiToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            if (isPost)
            {
                var uriBuilder = new UriBuilder(new Uri(client.BaseAddress, mapping.Service));
                var postData = mapping.JsonPostData?.ToString() ?? string.Empty;
                return await client.PostAsync(uriBuilder.Uri, new StringContent(postData), cancellationToken ?? CancellationToken.None);
            }
            else
            {
                var uriBuilder = new UriBuilder(new Uri(client.BaseAddress, mapping.Service)) { Query = mapping.QueryString };
                return await client.GetAsync(uriBuilder.Uri, cancellationToken ?? CancellationToken.None);
            }
        }
    }
}