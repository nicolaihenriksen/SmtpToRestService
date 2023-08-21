using System;
using System.IO;
using System.Reflection;

namespace SmtpToRest.Config;

public class DefaultConfigurationFileReader : IConfigurationFileReader
{
	public string Read()
	{
		string configFilePath = Path.Combine(Assembly.GetExecutingAssembly().Location, Configuration.Filename);
		if (!File.Exists(configFilePath))
		{
			throw new InvalidOperationException($"'{Configuration.Filename}' could not be found at specified path: '{configFilePath}'");
		}
		return File.ReadAllText(configFilePath);
	}
}