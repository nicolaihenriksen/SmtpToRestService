namespace SmtpToRest.Processing;

public class ProcessResult
{
    public bool IsSuccess { get; } = true;
    public string? Error { get; }

    private ProcessResult(string? error = null)
    {
        Error = error;
    }

    public static ProcessResult Success()
        => new ProcessResult();

    public static ProcessResult Failure(string error)
        => new ProcessResult(error);
}