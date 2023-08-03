using LaXiS.ImapLib;

namespace LaXiS.BackImapUp;

internal class Program
{
    static async Task Main(string[] args)
    {
        var client = new ImapClient("outlook.office365.com");

        await client.ConnectAsync();
        await client.GetCapabilitiesAsync();
        await client.NoopAsync();
        await client.LogoutAsync();
    }
}