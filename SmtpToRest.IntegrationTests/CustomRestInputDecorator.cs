using System;
using SmtpToRest.Config;
using SmtpToRest.Rest;
using SmtpToRest.Rest.Decorators;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.IntegrationTests;

internal class CustomRestInputDecorator : IRestInputDecorator
{
    private readonly Action<RestInput, ConfigurationMapping, IMimeMessage> _decoratorAction;

    public CustomRestInputDecorator(Action<RestInput, ConfigurationMapping, IMimeMessage> decoratorAction)
    {
        _decoratorAction = decoratorAction;
    }

    public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
    {
        _decoratorAction(restInput, mapping, message);
    }
}