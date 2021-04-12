using System;
using SmtpServer;

namespace SmtpToRestService
{
    internal interface ISmtpServerFactory
    {
        ISmtpServer Create(ISmtpServerOptions options, IServiceProvider serviceProvider);
    }
}