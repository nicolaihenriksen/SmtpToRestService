using SmtpToRest.Config;

namespace SmtpToRest;

public class SmtpRelayOptions
{
	public bool Enabled { get; set; } = false;
	public int Port { get; set; } = 465;
	public string? Host { get; set; }
	public bool Authenticate { get; set; } = true;
	public string? Username { get; set; }
	public string? Password { get; set; }

	public SmtpRelayOptions() { }

	internal SmtpRelayOptions(SmtpRelayOptions original)
	{
		Enabled = original.Enabled;
		Port = original.Port;
		Host = original.Host;
		Authenticate = original.Authenticate;
		Username = original.Username;
		Password = original.Password;
	}

	internal void Override(SmtpRelayConfiguration? config)
	{
		if (config is null)
			return;

		Enabled = config.Enabled ?? Enabled;
		Host = config.Host ?? Host;
		Port = config.Port ?? Port;
		Authenticate = config.Authenticate ?? Authenticate;
		Username = config.Username ?? Username;
		Password = config.Password ?? Password;
	}
}