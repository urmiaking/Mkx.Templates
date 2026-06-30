using System;

namespace Mkx.Templates.Shared.Utilities;

public static class Base64Url
{
    public static string Encode(byte[] input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static byte[] Decode(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        string incoming = input
            .Replace('-', '+')
            .Replace('_', '/');

        switch (incoming.Length % 4)
        {
            case 2: incoming += "=="; break;
            case 3: incoming += "="; break;
        }

        return Convert.FromBase64String(incoming);
    }
}
