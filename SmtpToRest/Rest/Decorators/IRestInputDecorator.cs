using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

public interface IRestInputDecorator
{
    RestInput Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message);
}

internal interface IRestInputDecoratorInternal
{
	RestInput Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message);
}