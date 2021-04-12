namespace SmtpToRestService
{
    internal interface IConfigurationFileReader
    {
        string Read(string path);
    }
}