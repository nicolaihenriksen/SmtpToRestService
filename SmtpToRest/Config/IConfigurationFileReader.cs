namespace SmtpToRest.Config;

public interface IConfigurationFileReader
{
	string Read(string path);
}