namespace SmtpToRest;

public class SmtpRelayOptions
{
	public int Port { get; set; } = 587;
	public string? Host { get; set; }
	public bool UseSsl { get; set; } = true;
	public string? Username { get; set; }
	public string? Password { get; set; }
}