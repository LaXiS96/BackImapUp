using LaXiS.ImapLib.Responses;

namespace LaXiS.ImapLib.Commands;

internal abstract class Command : IDisposable
{
    private readonly SemaphoreSlim _responseSemaphore = new(0, 1);
    private readonly List<Response> _responses = new();

    public string Tag { get; }
    public IReadOnlyList<Response> Responses => _responses;

    protected Command()
    {
        // TODO tag should be uniquely generated in the context of the client instance/connection
        // TODO are tags case insensitive? what about non-alphanumeric chars?
        Tag = "A001";
    }

    public AcceptResult Accept(Response response)
    {
        var result = InternalAccept(response);

        if (result is AcceptResult.Accepted or AcceptResult.Completed)
            _responses.Add(response);

        if (result is AcceptResult.Completed)
            _responseSemaphore.Release();

        return result;
    }

    public void Dispose()
    {
        _responseSemaphore.Dispose();
    }

    public abstract string GetPayload();

    /// <returns><see langword="true"/> if command is completed, <see langword="false"/> otherwise</returns>
    protected abstract AcceptResult InternalAccept(Response response);

    public Task WaitForResponsesAsync()
        => _responseSemaphore.WaitAsync();

    internal enum AcceptResult
    {
        /// <summary>Command did not accept the given response, probably because it was unrelated or unsolicited</summary>
        NotAccepted,
        /// <summary>Command accepted the response, but there may be more data incoming</summary>
        Accepted,
        /// <summary>Command accepted the response and has completed</summary>
        Completed,
    }

    internal enum CommandIdentifier
    {
        Capability,
        Noop,
        Logout,
    }
}
