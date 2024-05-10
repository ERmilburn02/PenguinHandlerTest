using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using PenguinHandlerTest;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

class TCPServer
{
    public static void Main()
    {
        // Console.WriteLine(Crypto.Hash("penis"));

        // DateTime now = DateTime.Now;

        // DateTime in5Mins = now.AddMinutes(5);

        // DateTime inAnHour = now.AddHours(1);

        // List<DateTime> times = [inAnHour, now, in5Mins];

        // times.Sort();

        // return;

        using var log = new LoggerConfiguration()
            .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
            .MinimumLevel.Verbose()
            .CreateLogger();
        Log.Logger = log;

        // Set the IP address and port number for the server
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 9999;

        // Create a TCP listener
        TcpListener listener = new TcpListener(ipAddress, port);

        Server server = new();

        try
        {
            Redis.Init();

            // Start listening for client requests
            listener.Start();

            Log.Information("Server is listening on port {Port}", port);

            while (true)
            {
                // Accept client connection
                TcpClient client = listener.AcceptTcpClient();
                Log.Verbose("Client connected");

                // Create a ClientHandler instance to handle client communication
                ClientHandler clientHandler = new ClientHandler(client, server);

                // Start handling client communication in a new thread
                Thread clientThread = new Thread(clientHandler.Handle);
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error: {Message}", ex.Message);
        }
        finally
        {
            // Stop listening for new clients
            listener.Stop();
        }
    }
}
