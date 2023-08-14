using System.Linq;
using MimeKit;

namespace SmtpToRest.Services.Smtp;

public class MimeMessageAdapter : IMimeMessage
{
    public string Address => _adaptee.From.OfType<MailboxAddress>().First().Address;

    private readonly MimeMessage _adaptee;

    public MimeMessageAdapter(MimeMessage adaptee)
    {
        _adaptee = adaptee;
    }
}