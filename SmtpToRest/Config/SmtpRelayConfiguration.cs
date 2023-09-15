namespace SmtpToRest.Config;

public class SmtpRelayConfiguration
{
	public int? Port { get; set; } = 587;
	public string? Host { get; set; }
	public string? Username { get; set; }
	public string? Password { get; set; }
	public bool? UseSsl { get; set; } = true;
}