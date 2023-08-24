using System;
using SmtpServer;

namespace SmtpToRest.Services.Smtp;

internal interface ISmtpServerFactory
{
    ISmtpServer Create(ISmtpServerOptions options, IServiceProvider serviceProvider);
}