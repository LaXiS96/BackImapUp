using LaXiS.ImapLib.Responses;
using static LaXiS.ImapLib.Responses.Response;

namespace LaXiS.ImapLib.Commands;

internal class GreetingCommand : Command
{
    public override string GetPayload()
    {
        throw new NotSupportedException("The implicit greeting does not have a payload");
    }

    protected override AcceptResult InternalAccept(Response response)
        => response is StatusResponse and
        {
            Tag: TagType.Untagged,
            Status: StatusType.Ok or StatusType.Bad or StatusType.Preauth
        }
        ? AcceptResult.Completed
        : AcceptResult.NotAccepted;
}
