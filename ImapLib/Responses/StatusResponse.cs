namespace LaXiS.ImapLib.Responses;

internal class StatusResponse : Response
{
    public StatusType Status { get; }
    public string Message { get; }

    internal StatusResponse(StatusType status, string message)
    {
        Status = status;
        Message = message;
    }
}
