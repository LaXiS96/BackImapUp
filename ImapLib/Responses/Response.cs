namespace LaXiS.ImapLib.Responses;

internal abstract class Response
{
    public TagType Tag { get; private set; }
    public string? CommandTag { get; private set; }

    public static Response Parse(string input)
    {
        var tagSplit = input.Split(' ', 3);

        // TODO data responses are never tagged
        var tag = tagSplit[0] switch
        {
            "*" => TagType.Untagged,
            "+" => TagType.Continuation,
            _ => TagType.Tagged,
        };
        string? commandTag = default;
        if (tag == TagType.Tagged)
            commandTag = tagSplit[0];

        Response response = tagSplit[1].ToUpperInvariant() switch
        {
            "OK" => new StatusResponse(StatusType.Ok, tagSplit[2]),
            "NO" => new StatusResponse(StatusType.No, tagSplit[2]),
            "BAD" => new StatusResponse(StatusType.Bad, tagSplit[2]),
            "PREAUTH" => new StatusResponse(StatusType.Preauth, tagSplit[2]),
            "BYE" => new StatusResponse(StatusType.Bye, tagSplit[2]),
            //"ENABLED"
            "CAPABILITY" => new CapabilityResponse(tagSplit[2]),
            //"LIST"
            //"NAMESPACE"
            //"STATUS"
            //"ESEARCH"
            //"FLAGS"
            // TODO EXISTS, EXPUNGE, FETCH have count before the identifier
            _ => throw new Exception($"Unknown response identifier '{tagSplit[1]}'"),
        };

        response.Tag = tag;
        response.CommandTag = commandTag;
        return response;
    }

    internal enum TagType
    {
        Untagged,
        Tagged,
        Continuation,
    }

    internal enum StatusType
    {
        Ok,
        No,
        Bad,
        Preauth,
        Bye,
    }
}
