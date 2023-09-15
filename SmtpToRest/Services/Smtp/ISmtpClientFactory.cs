namespace SmtpToRest.Services.Smtp;

internal interface ISmtpClientFactory
{
	ISmtpClient Create();
}