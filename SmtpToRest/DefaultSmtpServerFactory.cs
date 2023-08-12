using System;
using SmtpServer;

namespace SmtpToRest
{
    public class DefaultSmtpServerFactory : ISmtpServerFactory
    {
        public ISmtpServer Create(ISmtpServerOptions options, IServiceProvider serviceProvider)
        {
            var adaptee = new SmtpServer.SmtpServer(options, serviceProvider);
            return new SmtpServerAdapter(adaptee);
        }
    }
}