namespace SmtpToRest.Config;

public interface IConfiguration
{
    string? SmtpHost { get; }
    int[]? SmtpPorts { get; }
    string? ApiToken { get; }
    string? Endpoint { get; }
    string? HttpMethod { get; }
    bool TryGetMapping(string key, out ConfigurationMapping? mapping);
    SmtpRelayConfiguration? SmtpRelay { get; }
}