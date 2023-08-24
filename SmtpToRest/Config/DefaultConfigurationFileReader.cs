using System;
using System.IO;

namespace SmtpToRest.Config;

public class DefaultConfigurationFileReader : IConfigurationFileReader
{
	public string Read(string path)
	{
		if (!File.Exists(path))
		{
			throw new InvalidOperationException($"'{Configuration.Filename}' could not be found at specified path: '{path}'");
		}
		return File.ReadAllText(path);
	}
}