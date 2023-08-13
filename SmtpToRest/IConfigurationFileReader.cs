namespace SmtpToRest;

public interface IConfigurationFileReader
{
    string Read(string path);
}