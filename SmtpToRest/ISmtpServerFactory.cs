using System;
using SmtpServer;

namespace SmtpToRest;

public interface ISmtpServerFactory
{
    ISmtpServer Create(ISmtpServerOptions options, IServiceProvider serviceProvider);
}