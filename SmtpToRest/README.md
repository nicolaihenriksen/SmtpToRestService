# SmtpToRest ("self-hosting")

SmtpToRest can be self-hosted using the standard .NET `HostBuilder` scheme. Self-hosting is useful is you want to integrate
the application into an exisiting solution/application, as it requires very little configuration to get started.

First thing you need is to add the nuget package:
```pwsh
PM> Install-Package SmtpToRest
```

In order to add the background service, you simply use the extension method on the `IServiceCollection`:

```csharp
serviceCollection.AddSmtpToRest();
```

In order for the application to do anything useful, you need to configure the application. This can be done in one of 3 ways:
* Placing a `configuration.json` file somewhere reachable from the host, and point the host at the file, or
* Injecting your own implementation of the `SmtpToRest.Config.IConfiguration` interface into the service collection, or
* Injecting your own implementation of the `SmtpToRest.Config.IConfiguration` interface into the configuration in during registration.

<br/>

The first approach will require you to point the host to the file using a `SmtpToRest.Config.ConfigurationProvider` like this:
```csharp
serviceCollection.AddSingleton<IConfigurationProvider>(sp => new ConfigurationProvider(
    () => Path.Combine(System.AppContext.BaseDirectory)));
```
The path provided here needs to point to the directory containing the `configuration.json` file.

<br/>

The second approach will require you to add your own implementation of `SmtpToRest.Config.IConfiguration` (e.g. `MyCustomConfiguration`) and inject it directly into the `IServiceCollection` (typically as a singleton):
```csharp
serviceCollection.AddSingleton<IConfiguration, MyCustomConfiguration>();
```
<br/>

The third approach is very similar to the second approach, but this requires you to have the configuration instance
available when the host is being built. You can then inject it via the configuration callback of the `AddSmtpToRest()` extension method:
```csharp
serviceCollection.AddSmtpToRest(options => 
{
    options.ConfigurationMode = ConfigurationMode.OptionInjection;
    options.Configuration = myCustomConfiguration;
});
```
<br/>

## Customization
The service is designed for extendability and thus allows you to inject your own version of various components if you need to,
and it also allows you to inject your own `DelegatingHandler` into the `HttpClient` pipeline being used to perform the REST calls.

The components which are easily replacable are:

`IConfiguration`<br/>
As shown above, this allows you full control over the configuration using your own class if needed.

`IConfigurationProvider`<br/>
As shown above, this allows you to modify the location where the `configuration.json` can be found. The library contains a
default implementation of this interface, `ConfigurationProvider`, which is probably sufficient for nearly all scenarios.

`IRestInputDecorator`<br/>
You can register multiple types implementing this interface, and they will be executed after the built-in decorators in the
order they are added in the `IServiceCollection`. Note that the built-in decorators can also be disabled via the
`options => {}` callback of the `.AddSmtpToRest(...)` call. Decorators are used to build up the input passed into the
`HttpClient` which ind the end will perform the REST call.

`ISmtpServerFactory`, `ISmtpServer`, `IMessageStoreFactory`<br/>
These interfaces allow you to completely swap-out the internal SMTP server implementation if you have that need for some reason.

## Adding custom `HttpClient` pipeline behavior
You can use a custom `DelegatingHandler` implementation to inject into the `HttpClient` pipeline if you want to add some
behavior to the outoing REST requests which is not possible via the configuration. This could be some custom authorization
scheme for example. It could also be used to apply a retry-policy to the REST calls in case the target endpoint is not
always available.

Here are a couple of examples of the above mentioned handlers:
```csharp
internal class CustomHeaderHttpMessageHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("CustomHeader", "<some API key or credentials-hash>");
	    return base.SendAsync(request, cancellationToken);
    }
}
```
This handler simply decorates each request with an additional header before sending the request further down the pipeline.

```csharp
internal class RetryHttpMessageHandler : DelegatingHandler
{
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy = Policy<HttpResponseMessage>
                                                                            .Handle<HttpRequestException>()
                                                                            .RetryAsync(3);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        PolicyResult<HttpResponseMessage> result = await _retryPolicy.ExecuteAndCaptureAsync(() => base.SendAsync(request, cancellationToken));
        if (result.Outcome == OutcomeType.Failure)
        {
            throw new HttpRequestException("Error sending request", result.FinalException);
        }
        return result.Result;
    }
}
```
This handler leverages the [Polly](https://github.com/App-vNext/Polly) nuget package to implement a very simplistic 3-retries
approach. Note that Polly has a lot of more resilient policies available and this should only serve as an example. 

To inject a handler like this into the pipeline, you will need to do 2 things:
* Register the handler as a transient service in the `IServiceCollection`, and
* Add the handler into the `IHttpClientBuilder` which is available via a callback in the `.AddSmtpToRest(...)` call.

<br/>

```csharp
// Register the handler in the DI container
serviceCollection.AddTransient<CustomHeaderHttpMessageHandler>();   
serviceCollection.AddSmtpToRest(options =>
{
    // configure the options you want
},
httpConfig =>
{
    // Register the handler in the HttpClient pipeline used by the SmtpToRest service
    httpConfig.AddHttpMessageHandler<CustomHeaderHttpMessageHandler>();
});
```

The `HttpClient` is, by default, added to the `IServiceCollection` using a default name of 'SmtpToRest'. If for some reason
that clashes with an existing named client you have, you can easily change it:
```csharp
serviceCollection.AddSmtpToRest(options => 
{
    options.HttpClientName = "CustomHttpClientName";
});
```

Alternatively, if you have an existing client which you want to leverage, you could tell SmtpToRest does not register a
client (factory) and point it to an existing one instead:
```csharp
serviceCollection.AddSmtpToRest(options => 
{
    options.HttpClientName = "ExistingHttpClientName";
    option.UseBuiltInHttpClientFactory = false;
});
```