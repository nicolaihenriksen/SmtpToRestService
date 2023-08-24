using System.Collections.Generic;
using System.Linq;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal class AggregateDecorator : DecoratorBase, IRestInputDecoratorInternal
{
	private readonly List<IRestInputDecorator> _decorators;

	public AggregateDecorator(IEnumerable<IRestInputDecorator> decorators)
	{
		_decorators = decorators?.ToList() ?? new List<IRestInputDecorator>();
	}

	public RestInput Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
		=> _decorators.Aggregate(restInput, (current, decorator) => decorator.Decorate(current, mapping, message));
}