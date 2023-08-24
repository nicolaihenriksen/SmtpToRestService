using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmtpToRest.Config;

internal class Configuration : IConfiguration
{
	public string? SmtpHost { get; private set; }
	public int[]? SmtpPorts { get; private set; }
	public string? ApiToken { get; private set; }
    public string? Endpoint { get; private set; }
    public string? HttpMethod { get; private set; }

    public const string Filename = "configuration.json";
    private readonly Dictionary<string, ConfigurationMapping> _mappings = new();
    private readonly ILogger<Configuration> _logger;
    private readonly IConfigurationFileReader _configurationFileReader;
    private readonly string _configurationPath;

    public Configuration(ILogger<Configuration> logger, IConfigurationProvider configurationProvider, IConfigurationFileReader configurationFileReader, bool watch = true)
    {
        _logger = logger;
        _configurationFileReader = configurationFileReader;
        string configDir = configurationProvider.GetConfigurationFileDirectory();
        _configurationPath = Path.Combine(configDir, Filename);
        ReloadConfiguration(false);

        if (watch)
        {
	        var fileSystemWatcher = new FileSystemWatcher(configDir)
	        {
		        Filter = Filename,
		        NotifyFilter = NotifyFilters.LastWrite
	        };
	        fileSystemWatcher.Changed += (_, args) =>
	        {
		        if (Equals(args.Name, Filename))
		        {
			        ReloadConfiguration();
		        }
	        };
	        fileSystemWatcher.EnableRaisingEvents = true;
        }
    }

    private void ReloadConfiguration(bool continueOnError = true)
    {
        lock (_mappings)
        {
            try
            {
                _mappings.Clear();
                var json = _configurationFileReader.Read(_configurationPath);
                var configRoot = JsonSerializer.Deserialize<ConfigurationRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                SmtpHost = configRoot?.SmtpHost;
                SmtpPorts = configRoot?.SmtpPorts;
                ApiToken = configRoot?.ApiToken;
                Endpoint = configRoot?.Endpoint;
                HttpMethod = configRoot?.HttpMethod;
                var mappings = configRoot?.Mappings ?? new List<ConfigurationMapping>();
                foreach (var mapping in mappings)
                {
                    if (!string.IsNullOrWhiteSpace(mapping.Key))
                    {
                        _mappings[mapping.Key] = mapping;
                    }
                }
            }
            catch (Exception ex)
            {
                if (continueOnError)
                {
                    _logger.LogError(FormattableString.Invariant($"Unable to read configuration file ({Filename}), continuing with previous configuration"), ex);
                }
                else
                {
                    _logger.LogError(FormattableString.Invariant($"Unable to read configuration file ({Filename}), stopping service."), ex);
                    throw;
                }
            }
        }
    }

    public bool TryGetMapping(string key, out ConfigurationMapping? mapping)
    {
        mapping = null;
        lock (_mappings)
        {
            if (_mappings.TryGetValue(key, out var fetchedMapping))
            {
                mapping = fetchedMapping;
                return true;
            }
            return false;
        }
    }
}