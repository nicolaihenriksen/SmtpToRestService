using System.Linq;
using MimeKit;

namespace SmtpToRest.Services.Smtp;

internal class MimeMessageAdapter : IMimeMessage
{
    public string Address => _adaptee.From.Mailboxes.First().Address;

    private readonly MimeMessage _adaptee;

    public MimeMessageAdapter(MimeMessage adaptee)
    {
        _adaptee = adaptee;
    }
}