using System.Collections.Generic;

public class Server
{
    public Dictionary<int, ClientHandler> PenguinsById { get; private set; } = [];
}