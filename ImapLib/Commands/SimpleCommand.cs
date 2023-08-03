using LaXiS.ImapLib.Responses;

namespace LaXiS.ImapLib.Commands;

internal class SimpleCommand : Command
{
    private readonly CommandIdentifier _identifier;

    public SimpleCommand(CommandIdentifier identifier)
        : base()
    {
        _identifier = identifier;
    }

    public override string GetPayload()
        => $"{Tag} {Enum.GetName(_identifier)?.ToUpperInvariant()}";

    protected override AcceptResult InternalAccept(Response response)
    {
        if (response is StatusResponse and { Tag: Response.TagType.Tagged } && response.CommandTag == Tag)
            return AcceptResult.Completed;

        if (_identifier == CommandIdentifier.Capability && response is CapabilityResponse
            || response is StatusResponse sr and { Tag: Response.TagType.Untagged } && sr.Status == Response.StatusType.Bye)
            return AcceptResult.Accepted;

        return AcceptResult.NotAccepted;
    }
}
