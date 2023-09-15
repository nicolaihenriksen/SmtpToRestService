namespace SmtpToRest.Services.Smtp;

public interface IMimeMessage
{
    string? FirstFromAddress { get; }

    string[]? FromAddresses { get; }

    string? FirstToAddress { get; }

    string[]? FirstAddresses { get; }
}