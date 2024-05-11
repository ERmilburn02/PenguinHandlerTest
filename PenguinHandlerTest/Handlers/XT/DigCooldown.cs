using System;
using Serilog;

public class DigCooldownHandler : IMessageHandler
{
    public void HandleMessage(string message, ClientHandler client)
    {
        Log.Verbose("Received digcooldown");

        var redis = Redis.GetDatabase();

        var lastDig = redis.StringGet($"houdini.last_dig.{client.Penguin.Id}");

        if (lastDig.HasValue)
        {
            // cooldown_remaining = max(0, 120 - (int(time.time()) - int(last_dig)))
            var cooldown = Math.Max(0, 120 - (DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds - ((int)lastDig)));

            client.SendXT(ServerToClientXTPackets.DigCooldown, cooldown.ToString());
            return;
        }

        client.SendXT(ServerToClientXTPackets.DigCooldown, 0.ToString());
        return;
    }
}