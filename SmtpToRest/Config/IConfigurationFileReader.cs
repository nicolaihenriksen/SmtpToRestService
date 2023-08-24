namespace SmtpToRest.Config;

internal interface IConfigurationFileReader
{
	string Read(string path);
}