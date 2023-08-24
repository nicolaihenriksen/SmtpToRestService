using System.Linq;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

public class AggregateDecorator : DecoratorBase, IRestInputDecorator
{
	private readonly IRestInputDecorator[] _decorators;

	public AggregateDecorator(params IRestInputDecorator[] decorators)
	{
		_decorators = decorators;
	}

	public RestInput Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
		=> _decorators.Aggregate(restInput, (current, decorator) => decorator.Decorate(current, mapping, message));
}