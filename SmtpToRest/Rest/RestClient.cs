﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpToRest.Rest;

internal class RestClient : IRestClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RestClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpResponseMessage> InvokeService(RestInput input, CancellationToken cancellationToken)
    {
        string? endpoint = input.Endpoint;
        if (endpoint is null)
            return new HttpResponseMessage(HttpStatusCode.NotFound);

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(endpoint);

        if (!string.IsNullOrEmpty(input.ApiToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", input.ApiToken);

        UriBuilder uriBuilder = new UriBuilder(new Uri(client.BaseAddress!, input.Service))
        {
	        Query = Uri.EscapeDataString(input.QueryString ?? string.Empty)
        };
        switch (input.HttpMethod)
        {
	        case HttpMethod.Post:
		        string content = input.Content ?? string.Empty;
		        return await client.PostAsync(uriBuilder.Uri, new StringContent(content), cancellationToken);
			default:
                return await client.SendAsync(new HttpRequestMessage(input.HttpMethod.ToSystemNetHttpMethod(), uriBuilder.Uri), cancellationToken);
		}
    }
}