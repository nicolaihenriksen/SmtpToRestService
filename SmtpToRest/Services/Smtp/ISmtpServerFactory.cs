using System;
using SmtpServer;

namespace SmtpToRest.Services.Smtp;

public interface ISmtpServerFactory
{
    ISmtpServer Create(ISmtpServerOptions options, IServiceProvider serviceProvider);
}