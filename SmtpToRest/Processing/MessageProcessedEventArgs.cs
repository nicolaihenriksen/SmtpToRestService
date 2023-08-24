using System;

namespace SmtpToRest.Processing;

internal class MessageProcessedEventArgs : EventArgs
{
    public ProcessResult ProcessResult { get; }

    public MessageProcessedEventArgs(ProcessResult processResult)
    {
        ProcessResult = processResult;
    }
}