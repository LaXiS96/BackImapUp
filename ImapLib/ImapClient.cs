using LaXiS.ImapLib.Commands;
using LaXiS.ImapLib.Responses;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace LaXiS.ImapLib;

public class ImapClient : IDisposable
{
    // IMAP4 https://www.ietf.org/rfc/rfc1730.html
    // IMAP4rev1 https://www.ietf.org/rfc/rfc3501.html
    // IMAP4rev2 https://www.ietf.org/rfc/rfc9051.html

    private static readonly Encoding _Encoding = new UTF8Encoding(false);

    private TcpClient? _tcpClient;
    private SslStream? _sslStream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private Task? _readerTask;
    private List<Command> _commands = new();

    public string Hostname { get; }

    public ImapClient(string hostname)
    {
        Hostname = hostname;
    }

    public async Task ConnectAsync()
    {
        _tcpClient = new TcpClient(Hostname, 993);
        _sslStream = new SslStream(_tcpClient.GetStream(), false);

        await _sslStream.AuthenticateAsClientAsync(Hostname);

        _reader = new StreamReader(_sslStream, _Encoding);
        _writer = new StreamWriter(_sslStream, _Encoding);
        _readerTask = Task.Factory
            .StartNew(ReceiveLoopAsync, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning)
            .Unwrap();

        // Wait for greeting
        using var greeting = new GreetingCommand();
        _commands.Add(greeting);
        await greeting.WaitForResponsesAsync();

        // TODO should read capabilities and enable as needed after login
    }

    public Task GetCapabilitiesAsync()
        => SendAsync(new SimpleCommand(Command.CommandIdentifier.Capability));

    public void Dispose()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _sslStream?.Dispose();
        _tcpClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task LogoutAsync()
        => SendAsync(new SimpleCommand(Command.CommandIdentifier.Logout));

    public Task NoopAsync()
        => SendAsync(new SimpleCommand(Command.CommandIdentifier.Noop));

    private async Task ReceiveLoopAsync()
    {
        // TODO cancellation?

        while (true)
        {
            try
            {
                var line = await _reader.ReadLineAsync();
                if (line is null)
                    break;

                Console.WriteLine(line);
                // TODO read octets based on count in first line (first read first line, then check if response requires additional reading)
                var response = Response.Parse(line);

                // TODO commands list allows pipelining commands, see RFC §5.5
                // TODO maybe remove list and keep a single thread-safe command
                var command = _commands.Count > 0 ? _commands[0] : null;
                var result = command?.Accept(response) ?? Command.AcceptResult.NotAccepted;
                Console.WriteLine($"--- {command} {result}");
                if (result is Command.AcceptResult.NotAccepted)
                {
                    // TODO log
                }
                else if (result is Command.AcceptResult.Completed)
                {
                    Debug.Assert(command is not null, "Only a valid command can return Completed");
                    _commands.Remove(command);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task SendAsync(Command command)
    {
        _commands.Add(command);
        var payload = command.GetPayload();
        await _writer.WriteAsync($"{payload}\r\n");
        await _writer.FlushAsync();
        Console.WriteLine(payload);
        await command.WaitForResponsesAsync();
    }
}
