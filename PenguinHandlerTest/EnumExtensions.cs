using System;

public static class EnumExtensions
{
    public static string ToIdString(this XTGroup group)
    {
        return group switch
        {
            XTGroup.Error => "e",
            XTGroup.Login => "l",
            _ => throw new Exception("Unknown group")
        };
    }
}