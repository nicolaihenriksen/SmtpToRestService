namespace SmtpToRest.Processing;

internal class ProcessResult
{
	public bool IsSuccess => string.IsNullOrWhiteSpace(Error);
    public string? Error { get; }

    private ProcessResult(string? error = null)
    {
        Error = error;
    }

    public static ProcessResult Success() => new();

    public static ProcessResult Failure(string error) => new(error);
}