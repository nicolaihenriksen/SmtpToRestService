using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

public interface IRestInputDecorator
{
    void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message);
}

internal interface IRestInputDecoratorInternal
{
	void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message);
}