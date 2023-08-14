using System;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace SmtpToRest.Docker
{
	public class DockerConfigurationFileReader : IConfigurationFileReader
	{
		private readonly IWebHostEnvironment _environment;

		public DockerConfigurationFileReader(IWebHostEnvironment environment)
		{
			_environment = environment;
		}

		public string Read(string path)
		{
			string configFilePath = Path.Combine(_environment.ContentRootPath, "config", Configuration.Filename);
			if (!File.Exists(configFilePath))
			{
				throw new InvalidOperationException($"'{Configuration.Filename}' could not be found at specified path: '{configFilePath}'");
			}
			return File.ReadAllText(configFilePath);
		}
	}
}
