using System;

namespace SmtpToRest.Config;

public class ConfigurationProvider : IConfigurationProvider
{
	private readonly string _path;

	public ConfigurationProvider(Func<string> pathResolver)
	{
		_path = pathResolver();
	}

	public string GetConfigurationFileDirectory() => _path;
}