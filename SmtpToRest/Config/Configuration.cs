using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SmtpToRest.Config;

public class Configuration : IConfiguration
{
    public string? ApiToken { get; private set; }
    public string? Endpoint { get; private set; }
    public string? HttpMethod { get; private set; }

    public const string Filename = "configuration.json";
    private readonly Dictionary<string, ConfigurationMapping> _mappings = new Dictionary<string, ConfigurationMapping>();
    private readonly ILogger<Configuration> _logger;
    private readonly IConfigurationFileReader _configurationFileReader;

    public Configuration(ILogger<Configuration> logger, IConfigurationFileReader configurationFileReader)
    {
        _logger = logger;
        _configurationFileReader = configurationFileReader;
        /*
        var fileSystemWatcher = new FileSystemWatcher(CurrentPath)
        {
            Filter = Filename,
            NotifyFilter = NotifyFilters.LastWrite
        };
        fileSystemWatcher.Changed += (sender, args) => ReloadConfiguration(args.Name!);
        fileSystemWatcher.EnableRaisingEvents = true;
        */
        ReloadConfiguration(Filename, false);
    }

    private void ReloadConfiguration(string filename, bool continueOnError = true)
    {
        if (Filename != filename)
            return;

        lock (_mappings)
        {
            try
            {
                _mappings.Clear();
                var json = _configurationFileReader.Read();
                var configRoot = JsonSerializer.Deserialize<ConfigurationRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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