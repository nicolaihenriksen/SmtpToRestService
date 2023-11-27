using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class ContentTokenReplacementDecorator : TokenReplacementDecoratorBase, IRestInputDecorator
{
    public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
    {
        restInput.Content = ReplaceTokens(restInput.Content, message);
    }
}