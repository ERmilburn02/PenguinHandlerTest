using System;

public static class EnumExtensions
{
    public static string ToIdString(this ServerToClientXTPackets group)
    {
        return group switch
        {
            ServerToClientXTPackets.Error => "e",
            ServerToClientXTPackets.Login => "l",
            ServerToClientXTPackets.DigCooldown => "getdigcooldown",
            _ => throw new Exception("Unknown packet")
        };
    }
}