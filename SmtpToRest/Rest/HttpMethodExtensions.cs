using System;

namespace SmtpToRest.Rest;

internal static class HttpMethodExtensions
{
	public static System.Net.Http.HttpMethod ToSystemNetHttpMethod(this HttpMethod method)
		=> method switch
		{
			HttpMethod.Post => System.Net.Http.HttpMethod.Post,
			HttpMethod.Get => System.Net.Http.HttpMethod.Get,
			HttpMethod.Delete => System.Net.Http.HttpMethod.Delete,
			HttpMethod.Head => System.Net.Http.HttpMethod.Head,
			HttpMethod.Options => System.Net.Http.HttpMethod.Options,
			HttpMethod.Patch => System.Net.Http.HttpMethod.Patch,
			HttpMethod.Put => System.Net.Http.HttpMethod.Put,
			HttpMethod.Trace => System.Net.Http.HttpMethod.Trace,
			_ => throw new ArgumentOutOfRangeException(nameof(method), method, "Conversion not supported for value"),
		};
}