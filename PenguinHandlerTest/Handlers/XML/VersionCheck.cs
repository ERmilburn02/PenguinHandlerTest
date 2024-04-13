using System;
using System.Xml;
using Serilog;

public class VersionCheckHandler : IMessageHandler
{
    public void HandleMessage(string message, ClientHandler client)
    {
        Log.Verbose("Received verChk");

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(message);

        // Get the root element
        XmlElement root = xmlDoc.DocumentElement;

        // Navigate to the 'body' element
        XmlNode bodyNode = root.SelectSingleNode("body");

        // Navigate to the 'ver' element
        XmlNode verNode = bodyNode.SelectSingleNode("ver");

        // Access the attributes of the 'ver' element
        string vAttribute = ((XmlElement)verNode).GetAttribute("v");
        Log.Verbose("Client Version: {Version}", vAttribute);

        try
        {
            int clientVersion = int.Parse(vAttribute);

            if (clientVersion == Config.LegacyClientVersion)
            {
                Log.Warning("Legacy client attempted to connect");
                client.SendXML("<body action='apiKO' r='0' />");
                client.Disconnect();
                return;
            }

            if (clientVersion == Config.VanillaClientVersion)
            {
                Log.Verbose("Vanilla client connected");
                client.SendXML("<body action='apiOK' r='0' />");
            }
        }
        catch (Exception _ex)
        {
            Log.Error(_ex, "Error: {Message}", _ex.Message);
            // They made us throw, we kick them
            client.Disconnect();
            throw;
        }

    }
}