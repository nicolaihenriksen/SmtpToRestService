using System.Linq;
using MimeKit;

namespace SmtpToRestService
{
    internal class MimeMessageAdapter : IMimeMessage
    {
        public string Address => _adaptee.From.OfType<MailboxAddress>().First().Address;

        private readonly MimeMessage _adaptee;

        public MimeMessageAdapter(MimeMessage adaptee)
        {
            _adaptee = adaptee;
        }
    }
}