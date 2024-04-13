using System;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using Serilog;

public class ClientHandler
{
    private TcpClient client;
    private NetworkStream stream;
    private bool isAlive;

    public ClientHandler(TcpClient client)
    {
        this.client = client;
        this.stream = client.GetStream();
        this.isAlive = true;
    }

    public void Handle()
    {
        try
        {
            while (isAlive)
            {
                string requestData = ReadMessage();

                // DEBUG
                Log.Verbose("Received data: {requestData}", requestData);

                if (string.IsNullOrEmpty(requestData))
                {
                    Log.Information("Received empty data, closing connection");
                    break;
                }

                if (requestData.StartsWith("%xt%"))
                {
                    HandleXtMessage(requestData);
                }
                else if (requestData.StartsWith('<'))
                {
                    HandleXmlMessage(requestData);
                }
                else
                {
                    Log.Error("Received unknown data: {requestData}", requestData);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error: {Message}", ex.Message);
        }
        finally
        {
            CloseConnection();
        }
    }

    private string ReadMessage()
    {
        return NetworkStreamReader.ReadNullTerminatedString(stream);
    }

    /// <summary>
    /// Sends a cross-domain policy file response
    /// </summary>
    private void SendPolicyFileResponse()
    {
        string responseData = "<cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"9999\" /></cross-domain-policy>";
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseData);
        stream.Write(responseBytes, 0, responseBytes.Length);
        stream.Flush();
        Log.Verbose("Sent cross-domain-policy response");
    }

    private void HandleXtMessage(string message)
    {
        // Handle %xt% data
        Log.Error("Unhandled XT message: {message}", message);
    }

    private void HandleXmlMessage(string message)
    {
        if (message.StartsWith("<policy-file-request/>"))
        {
            SendPolicyFileResponse();
            isAlive = false; // Close the connection after sending the Policy File
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(message);

        // Get the root element
        XmlElement root = xmlDoc.DocumentElement;

        // Get the body element
        XmlNode bodyNode = root.SelectSingleNode("body");

        // Get the action attribute
        string actionAttribute = ((XmlElement)bodyNode).GetAttribute("action");

        switch (actionAttribute)
        {
            case "verChk":
                new VersionCheckHandler().HandleMessage(message, this);
                return;

            case "rndK":
                new RandomKeyHandler().HandleMessage(message, this);
                return;

            default:
                Log.Error("Unhandled XML message of type {actionAttribute}: {message}", actionAttribute, message);
                return;
        }
    }

    private void CloseConnection()
    {
        stream.Close();
        client.Close();
        isAlive = false;
        Log.Verbose("Connection closed");
    }

    public void Disconnect()
    {
        isAlive = false;
        // We let the handle loop automatically close the connection
    }

    public void SendXML(string message)
    {
        string messageToSend = string.Format("<msg t='sys'>{0}</msg>\0", message);

        Log.Verbose("Sending data: {messageToSend}", messageToSend[..^1]);

        byte[] responseBytes = Encoding.UTF8.GetBytes(messageToSend);
        stream.Write(responseBytes, 0, responseBytes.Length);
        stream.Flush();
    }
}
