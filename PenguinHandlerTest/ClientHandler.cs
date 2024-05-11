using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using PenguinHandlerTest;
using Serilog;

public class ClientHandler
{
    private TcpClient _client;
    private NetworkStream _stream;
    private bool _isAlive;
    private Server _server;
    private Penguin _penguin;

    public Server Server => _server;
    public Penguin Penguin => _penguin;

    public ClientHandler(TcpClient client, Server server)
    {
        this._client = client;
        this._stream = client.GetStream();
        this._isAlive = true;
        this._server = server;
    }

    /// <summary>
    /// Handles incoming messages from the client, processes them based on their format, and responds accordingly.
    /// </summary>
    public void Handle()
    {
        try
        {
            while (_isAlive)
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
        return NetworkStreamReader.ReadNullTerminatedString(_stream);
    }

    /// <summary>
    /// Sends a cross-domain policy file response
    /// </summary>
    private void SendPolicyFileResponse()
    {
        string responseData = "<cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"9999\" /></cross-domain-policy>";
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseData);
        _stream.Write(responseBytes, 0, responseBytes.Length);
        _stream.Flush();
        Log.Verbose("Sent cross-domain-policy response");
    }

    private void HandleXtMessage(string message)
    {
        // Handle %xt% data

        // XT Packets are in the format "%xt%s%{group}#{action}%{data}%"
        // We need to extract the group and the action
        // 0 - empty, 1 - xt, 2 - s, 3 - {group}#{action}, 4+ - {data}, last - empty
        string[] args = message.Split('%')[3].Split('#');
        string group = args[0];
        string action = args[1];

        // TODO: Switch on group, and let each group handle the action

        switch (action)
        {
            case "getdigcooldown":
                new DigCooldownHandler().HandleMessage(message, this);
                return;

            default:
                Log.Error("Unhandled XT message with group {group} and action {action}: {message}", group, action, message);
                return;
        }

    }

    private void HandleXmlMessage(string message)
    {
        if (message.StartsWith("<policy-file-request/>"))
        {
            SendPolicyFileResponse();
            _isAlive = false; // Close the connection after sending the Policy File
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

            case "login":
                new LoginHandler().HandleMessage(message, this);
                return;

            default:
                Log.Error("Unhandled XML message of type {actionAttribute}: {message}", actionAttribute, message);
                return;
        }
    }

    private void CloseConnection()
    {
        _stream.Close();
        _client.Close();
        _isAlive = false;
        Log.Verbose("Connection closed");
    }

    public void Disconnect()
    {
        _isAlive = false;
        // We let the handle loop automatically close the connection
    }

    public void SendXML(string message)
    {
        string messageToSend = string.Format("<msg t='sys'>{0}</msg>\0", message);

        Log.Verbose("Sending data: {messageToSend}", messageToSend[..^1]);

        byte[] responseBytes = Encoding.UTF8.GetBytes(messageToSend);
        _stream.Write(responseBytes, 0, responseBytes.Length);
        _stream.Flush();
    }

    public void SendXT(ServerToClientXTPackets group, params string[] args)
    {
        var groupId = group.ToIdString();
        var argString = string.Join("%", args);
        string internalId = "-1"; // No clue what this is, but houdini says it's always -1

        string messageToSend = string.Format("%xt%{0}%{1}%{2}%\0", groupId, internalId, argString);

        Log.Verbose("Sending data: {messageToSend}", messageToSend[..^1]);

        byte[] responseBytes = Encoding.UTF8.GetBytes(messageToSend);
        _stream.Write(responseBytes, 0, responseBytes.Length);
        _stream.Flush();
    }

    public void SendErrorAndDisconnect(Error error, params string[] args)
    {
        SendErrorAndDisconnect((int)error, args);
    }

    public void SendErrorAndDisconnect(int error, params string[] args)
    {
        string[] newArgs = new string[args.Length + 1];
        newArgs[0] = error.ToString();
        Array.Copy(args, 0, newArgs, 1, args.Length);
        SendXT(ServerToClientXTPackets.Error, newArgs);
        Disconnect();
    }

    public void SetPenguin(Penguin penguin)
    {
        _penguin = penguin;
    }
}
