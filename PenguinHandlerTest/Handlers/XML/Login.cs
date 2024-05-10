using System;
using System.Linq;
using System.Xml;
using PenguinHandlerTest;
using Serilog;

public class LoginHandler : IMessageHandler
{

    private class NickData
    {
        public int Id;
        public string Username;
        public string LoginKey;
        public string SecretKey;
        public bool Approved;
        public bool Rejected;
    }

    private class PasswordData
    {
        public string ClientKey;
        public string ConfirmationHash;
    }

    public void HandleMessage(string message, ClientHandler client)
    {
        Log.Verbose("Received login");

        XmlDocument xmlDoc = new();
        xmlDoc.LoadXml(message);

        // Get the root element
        XmlElement root = xmlDoc.DocumentElement;

        // Navigate to the 'body' element
        XmlNode bodyNode = root.SelectSingleNode("body");

        // Navigate to the 'login' element
        XmlNode loginNode = bodyNode.SelectSingleNode("login");

        // Navigate to the 'nick' element
        XmlNode nickNode = loginNode.SelectSingleNode("nick");

        // Navigate to the 'pword' element
        XmlNode pwordNode = loginNode.SelectSingleNode("pword");

        // Get the inner text of the 'nick' element
        string nickInnerText = nickNode.InnerText;
        string pwordInnerText = pwordNode.InnerText;


        string[] nickDataArr = nickInnerText.Split('|');

        NickData nickData = new()
        {
            Id = int.Parse(nickDataArr[0]),
            Username = nickDataArr[2],
            LoginKey = nickDataArr[3],
            SecretKey = nickDataArr[4],
            Approved = int.Parse(nickDataArr[5]) == 1,
            Rejected = int.Parse(nickDataArr[6]) == 1
        };

        string[] pwordDataArr = pwordInnerText.Split('#');
        PasswordData passwordData = new()
        {
            ClientKey = pwordDataArr[0],
            ConfirmationHash = pwordDataArr[1]
        };

        var redis = Redis.GetDatabase();
        var loginKeyRedisValue = redis.StringGet($"{nickData.Username}.lkey");
        var confirmHashRedisValue = redis.StringGet($"{nickData.Username}.ckey");

        redis.KeyDelete($"{nickData.Username}.lkey");
        redis.KeyDelete($"{nickData.Username}.ckey");

        if (!loginKeyRedisValue.HasValue || !confirmHashRedisValue.HasValue)
        {
            client.Disconnect();
            return;
        }

        var loginKey = loginKeyRedisValue.ToString();
        var confirmHash = confirmHashRedisValue.ToString();

        if (loginKey != nickData.LoginKey || confirmHash != passwordData.ConfirmationHash)
        {
            Log.Verbose("\"{Username}\" attempted to connect with invalid credentials", nickData.Username);
            client.Disconnect();
            return;
        }

        var loginHash = Crypto.EncryptPassword(loginKey + Config.AuthKey) + loginKey;
        if (passwordData.ClientKey != loginHash)
        {
            Log.Verbose("\"{Username}\" attempted to connect, but the client key is invalid", nickData.Username);

            client.Disconnect();
            return;
        }

        // At this point, we've verified that they're authenticated

        // Get the full player data from the db
        using var db = new PostgresContext();
        var player = db.Penguins.Where(p => p.Username == nickData.Username).FirstOrDefault() ?? throw new Exception($"Player {nickData.Username} not found in database! This should never happen! Check Login Server logs!");

        // Place the login key back in redis for other services (CJ Snow, etc...)
        redis.StringSet($"{player.Username}.loginkey", loginKey, TimeSpan.FromHours(12));

        if (client.Server.PenguinsById.Count >= Config.MaximumPlayers && !player.Moderator)
        {
            Log.Warning("\"{Username}\" attempted to connect when the server is full", nickData.Username);
            client.SendErrorAndDisconnect(Error.ServerFull);
            return;
        }

        if (player.Permaban)
        {
            Log.Verbose("\"{Username}\" attempted to connect when they are permanently banned", nickData.Username);
            client.SendErrorAndDisconnect(Error.PermaBanned);
            return;
        }

        var timeNow = DateTime.UtcNow;
        var timeWithoutType = new DateTime(timeNow.Ticks, DateTimeKind.Unspecified);


        var ban = db.Bans.Where(b => b.PenguinId == player.Id && b.Expires > timeWithoutType).OrderByDescending(b => b.Expires).FirstOrDefault();

        if (ban != null)
        {
            Console.WriteLine(ban.Expires.Kind);

            Log.Verbose("\"{Username}\" attempted to connect when they are banned", nickData.Username);

            client.SendErrorAndDisconnect(Error.Banned, Math.Round((ban.Expires - timeWithoutType).TotalHours).ToString());
            return;
        }

        if (client.Server.PenguinsById.ContainsKey(player.Id))
        {
            Log.Warning("\"{Username}\" attempted to connect when they are already connected", nickData.Username);
            client.Server.PenguinsById[player.Id].Disconnect();
        }

        // TODO: Update local player info

        client.SendXT(XTGroup.Login);

        return;
    }
}