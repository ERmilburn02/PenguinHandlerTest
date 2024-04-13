using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

public class NetworkStreamReader
{
    public static string ReadNullTerminatedString(NetworkStream stream)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            int character;
            while ((character = stream.ReadByte()) != 0)
            {
                if (character == -1)
                {
                    // End of stream reached, return current data
                    return Encoding.UTF8.GetString(memoryStream.ToArray());
                }

                // Append character to memory stream
                memoryStream.WriteByte((byte)character);
            }

            // Null byte encountered, return current data
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
