using System;

namespace SmtpToRest.Processing;

public class MessageProcessedEventArgs : EventArgs
{
    public ProcessResult ProcessResult { get; }

    public MessageProcessedEventArgs(ProcessResult processResult)
    {
        ProcessResult = processResult;
    }
}