using Serilog;

public class RandomKeyHandler : IMessageHandler
{
    public void HandleMessage(string message, ClientHandler client)
    {
        Log.Verbose("Received rndK");

        // We don't need to parse the message, just send back the auth key
        client.SendXML($"<body action='rndK' r='-1'><k>{Config.AuthKey}</k></body>");
    }
}