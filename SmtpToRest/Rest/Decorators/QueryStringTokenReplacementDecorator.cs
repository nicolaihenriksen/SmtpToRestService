using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class QueryStringTokenReplacementDecorator : TokenReplacementDecoratorBase, IRestInputDecorator
{
    public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
    {
        restInput.QueryString = ReplaceTokens(restInput.QueryString, message);
    }
}