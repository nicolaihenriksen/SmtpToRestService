namespace SmtpToRest.Config;

public class SmtpRelayConfiguration
{
	public bool? Enabled { get; set; }
	public int? Port { get; set; }
	public string? Host { get; set; }
	public bool? Authenticate { get; set; }
	public string? Username { get; set; }
	public string? Password { get; set; }
}