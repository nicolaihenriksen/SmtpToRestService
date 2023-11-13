using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class EndpointTokenReplacementDecorator : TokenReplacementDecoratorBase, IRestInputDecorator
{
    public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
    {
        restInput.Endpoint = ReplaceTokens(restInput.Endpoint, message);
    }
}