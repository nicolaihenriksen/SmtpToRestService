using System;
using System.Globalization;
using System.Text.RegularExpressions;
using SmtpToRest.Services.Smtp;

namespace SmtpToRest.Rest.Decorators;

internal abstract class TokenReplacementDecoratorBase : DecoratorBase
{
    private const string FromToken = "from";
    private const string ToToken = "to";
    private const string BodyToken = "body";

    private static readonly Regex FromRegex = new(GetTokenPattern(FromToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex FromStartIndexAndOptionalLengthRegex = new(GetStartIndexAndOptionalLengthPattern(FromToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex FromIndexOfStringAndOptionalLengthRegex = new(GetIndexOfStringAndOptionalLengthPattern(FromToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex FromIndexOfStringToIndexOfStringRegex = new(GetIndexOfStringToIndexOfStringPattern(FromToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ToRegex = new(GetTokenPattern(ToToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ToStartIndexAndOptionalLengthRegex = new(GetStartIndexAndOptionalLengthPattern(ToToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ToIndexOfStringAndOptionalLengthRegex = new(GetIndexOfStringAndOptionalLengthPattern(ToToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ToIndexOfStringToIndexOfStringRegex = new(GetIndexOfStringToIndexOfStringPattern(ToToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BodyRegex = new(GetTokenPattern(BodyToken), RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BodyStartIndexAndOptionalLengthRegex = new(GetStartIndexAndOptionalLengthPattern(BodyToken), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex BodyIndexOfStringAndOptionalLengthRegex = new(GetIndexOfStringAndOptionalLengthPattern(BodyToken), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex BodyIndexOfStringToIndexOfStringRegex = new(GetIndexOfStringToIndexOfStringPattern(BodyToken), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private static string GetTokenPattern(string placeholder)
    {
        return @$"(\$\({placeholder}\))";
    }
    private static string GetStartIndexAndOptionalLengthPattern(string placeholder)
    {
        return @$"(\$\({placeholder}\))(\{{(\d+)(?:,(\d+))?\}})";
    }
    private static string GetIndexOfStringAndOptionalLengthPattern(string placeholder)
    {
        return @$"(\$\({placeholder}\))\{{\[(.*)\]([+-]{"{1}"}\d+)?(?:,(\d+))?\}}";
    }
    private static string GetIndexOfStringToIndexOfStringPattern(string placeholder)
    {
        return @$"(\$\({placeholder}\))\{{\[(.*)\]([+-]{"{1}"}\d+)?,\[(.*)\]([+-]{"{1}"}\d+)?\}}";
    }

    private static string? ReplaceToken(Regex startIndexAndOptionalLengthRegex,
        Regex indexOfStringToIndexOfStringRegex,
        Regex indexOfStringAndOptionalLengthRegex,
        Regex tokenOnlyRegex, string? input, string? tokenContent)
    {
        if (input is null || tokenContent is null)
            return input;

        if (startIndexAndOptionalLengthRegex.Match(input) is { Success: true } match1)
        {
            int startIndex = Convert.ToInt32(match1.Groups[3].Value);
            int? length = null;
            if (int.TryParse(match1.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int l))
                length = l;

            return input.Replace(match1.Value, !length.HasValue
                ? tokenContent[startIndex..]
                : tokenContent.Substring(startIndex, length.Value), StringComparison.InvariantCultureIgnoreCase);
        }
        if (indexOfStringToIndexOfStringRegex.Match(input) is { Success: true } match2)
        {
            int startIndex = Convert.ToInt32(tokenContent.IndexOf(match2.Groups[2].Value, StringComparison.InvariantCultureIgnoreCase));
            if (int.TryParse(match2.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int startOffset))
                startIndex += startOffset;

            int endIndex = Convert.ToInt32(tokenContent.IndexOf(match2.Groups[4].Value, StringComparison.InvariantCultureIgnoreCase));
            if (int.TryParse(match2.Groups[5].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int endOffset))
                endIndex += endOffset;

            return input.Replace(match2.Value, tokenContent[startIndex..endIndex], StringComparison.InvariantCultureIgnoreCase);
        }
        if (indexOfStringAndOptionalLengthRegex.Match(input) is { Success: true } match3)
        {
            int startIndex = Convert.ToInt32(tokenContent.IndexOf(match3.Groups[2].Value, StringComparison.InvariantCultureIgnoreCase));
            if (int.TryParse(match3.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int offset))
                startIndex += offset;

            int? length = null;
            if (int.TryParse(match3.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int l))
                length = l;

            return input.Replace(match3.Value, !length.HasValue
                ? tokenContent[startIndex..]
                : tokenContent.Substring(startIndex, length.Value), StringComparison.InvariantCultureIgnoreCase);
        }
        if (tokenOnlyRegex.Match(input) is { Success: true } match4)
        {
            return input.Replace(match4.Value, tokenContent, StringComparison.InvariantCultureIgnoreCase);
        }
        return input;
    }

    private static string? ReplaceFromToken(string? input, string? from)
    {
        return ReplaceToken(
            FromStartIndexAndOptionalLengthRegex,
            FromIndexOfStringToIndexOfStringRegex,
            FromIndexOfStringAndOptionalLengthRegex,
            FromRegex,
            input,
            from);
    }

    private static string? ReplaceToToken(string? input, string? to)
    {
        return ReplaceToken(
            ToStartIndexAndOptionalLengthRegex,
            ToIndexOfStringToIndexOfStringRegex,
            ToIndexOfStringAndOptionalLengthRegex,
            ToRegex,
            input,
            to);
    }

    private static string? ReplaceBodyToken(string? input, string? body)
    {
        return ReplaceToken(
            BodyStartIndexAndOptionalLengthRegex,
            BodyIndexOfStringToIndexOfStringRegex,
            BodyIndexOfStringAndOptionalLengthRegex,
            BodyRegex,
            input,
            body);
    }

    protected static string? ReplaceTokens(string? input, IMimeMessage message)
    {
        return ReplaceFromToken(ReplaceToToken(ReplaceBodyToken(input, message.BodyAsString), message.FirstToAddress), message.FirstFromAddress);
    }
}