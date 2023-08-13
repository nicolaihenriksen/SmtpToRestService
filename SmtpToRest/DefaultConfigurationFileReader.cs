using System.IO;

namespace SmtpToRest;

public class DefaultConfigurationFileReader : IConfigurationFileReader
{
    public string Read(string path)
    {
        return File.ReadAllText(path);
    }
}