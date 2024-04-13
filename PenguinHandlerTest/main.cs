using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

class TCPServer
{
    public static void Main()
    {
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

        try
        {
            // Start listening for client requests
            listener.Start();

            Log.Information("Server is listening on port {Port}", port);

            while (true)
            {
                // Accept client connection
                TcpClient client = listener.AcceptTcpClient();
                Log.Verbose("Client connected");

                // Create a ClientHandler instance to handle client communication
                ClientHandler clientHandler = new ClientHandler(client);

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
