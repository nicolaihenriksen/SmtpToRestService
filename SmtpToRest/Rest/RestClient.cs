using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest.Rest;

internal class RestClient : IRestClient
{
	private readonly IHttpClientConfiguration _httpClientConfiguration;
	private readonly IHttpClientFactory _httpClientFactory;

    public RestClient(IHttpClientConfiguration httpClientConfiguration, IHttpClientFactory httpClientFactory)
    {
	    _httpClientConfiguration = httpClientConfiguration;
	    _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpResponseMessage> InvokeService(RestInput input, CancellationToken cancellationToken)
    {
        string? endpoint = input.Endpoint;
        if (endpoint is null)
            return new(HttpStatusCode.NotFound);

        var client = _httpClientFactory.CreateClient(input.HttpClientName ?? _httpClientConfiguration.HttpClientName);
        client.BaseAddress = new(endpoint);

        if (!string.IsNullOrEmpty(input.ApiToken))
            client.DefaultRequestHeaders.Authorization = new("Bearer", input.ApiToken);

		UriBuilder uriBuilder = new(new Uri(client.BaseAddress!, input.Service))
        {
	        Query = EscapeQueryString(input.QueryString ?? string.Empty)
        };
        switch (input.HttpMethod)
        {
	        case HttpMethod.Post:
		        string content = input.Content ?? string.Empty;
		        return await client.PostAsync(uriBuilder.Uri, new StringContent(content), cancellationToken);
	        case HttpMethod.Put:
		        content = input.Content ?? string.Empty;
		        return await client.PutAsync(uriBuilder.Uri, new StringContent(content), cancellationToken);
	        case HttpMethod.Patch:
		        content = input.Content ?? string.Empty;
		        return await client.PatchAsync(uriBuilder.Uri, new StringContent(content), cancellationToken);
			default:
                return await client.SendAsync(new(input.HttpMethod.ToSystemNetHttpMethod(), uriBuilder.Uri), cancellationToken);
		}
    }

    // Simplistic escape of query string; should probably be refactored at some point.
    private static string EscapeQueryString(string queryString)
    {
        string[] keyValuePairs = queryString.Split('&');
        for (int i = 0; i < keyValuePairs.Length; i++)
        {
            string[] keyValuePair = keyValuePairs[i].Split('=');
			if (keyValuePair.Length != 2)
				continue;

            keyValuePairs[i] = $"{Uri.EscapeDataString(keyValuePair[0])}={Uri.EscapeDataString(keyValuePair[1])}";
		}
        return string.Join("&", keyValuePairs);
    }
}