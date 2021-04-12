using System.IO;

namespace SmtpToRestService
{
    internal class DefaultConfigurationFileReader : IConfigurationFileReader
    {
        public string Read(string path)
        {
            return File.ReadAllText(path);
        }
    }
}