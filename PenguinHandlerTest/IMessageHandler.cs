public interface IMessageHandler
{
    void HandleMessage(string message, ClientHandler client);
}