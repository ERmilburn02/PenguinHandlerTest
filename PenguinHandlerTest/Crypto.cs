using System.Text;
using System.Security.Cryptography;
using System;

public static class Crypto
{
    public static string Hash(int i)
    {
        return Hash(i.ToString());
    }

    public static string Hash(string s)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(s));
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    public static string EncryptPassword(string password, bool alreadyHashed = false)
    {
        if (!alreadyHashed)
        {
            password = Hash(password);
        }

        var swappedHash = password[16..32] + password[0..16];
        return swappedHash;
    }
}