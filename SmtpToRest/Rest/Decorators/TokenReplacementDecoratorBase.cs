using System;
using System.Text.RegularExpressions;
using SmtpToRest.Config;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal abstract class TokenReplacementDecoratorBase : DecoratorBase
{
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    //private static readonly Regex FromAddressRegex = new(@"(\$\(from\))(\{(\d+),(\d+)\})?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    //private static readonly Regex ToAddressRegex = new(@"(\$\(to\))(\{(\d+),(\d+)\})?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BodyRegex = new(@"(\$\(body\))(\{(\d+)(,(\d+))*\})?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

    protected static string? ReplaceBodyToken(string? input, string? body)
    {
        if (input is null || body is null)
            return input;

        Match match = BodyRegex.Match(input);
        if (!match.Success)
            return input;

        if (string.IsNullOrWhiteSpace(match.Groups[2].Value))
        {
            return input.Replace(match.Value, body, StringComparison.InvariantCultureIgnoreCase);
        }
        if (!int.TryParse(match.Groups[3].Value, out int startIndex) || startIndex > body.Length)
        {
            return input;
        }
        if (string.IsNullOrWhiteSpace(match.Groups[5].Value))
        {
            return input.Replace(match.Value, body[startIndex..], StringComparison.InvariantCultureIgnoreCase);
        }
        if (int.TryParse(match.Groups[5].Value, out int length) && (startIndex + length <= body.Length))
        {
            return input.Replace(match.Value, body.Substring(startIndex, length), StringComparison.InvariantCultureIgnoreCase);
        }
        return input;
    }
}

internal class EndpointTokenReplacementDecorator : TokenReplacementDecoratorBase, IRestInputDecorator
{
    public void Decorate(RestInput restInput, ConfigurationMapping mapping, IMimeMessage message)
    {
        restInput.Endpoint = ReplaceBodyToken(restInput.Endpoint, message.BodyAsString);
    }
}