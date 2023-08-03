namespace LaXiS.ImapLib.Responses;

internal class CapabilityResponse : Response
{
    public string Capabilities { get; private set; }

    internal CapabilityResponse(string capabilities)
    {
        Capabilities = capabilities;
    }
}
