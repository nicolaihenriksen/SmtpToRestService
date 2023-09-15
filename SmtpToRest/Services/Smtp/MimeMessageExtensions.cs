namespace SmtpToRest.Services.Smtp;

internal static class MimeMessageExtensions
{
	public static MimeKit.MimeMessage? ToMimeKitMimeMessage(this IMimeMessage mimeMessage)
	{
		if (mimeMessage is MimeMessageAdapter adapter)
		{
			return adapter;
		}
		return null;
	}
}