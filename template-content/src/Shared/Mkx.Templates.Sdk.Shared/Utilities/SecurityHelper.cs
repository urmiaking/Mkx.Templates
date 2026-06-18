using System;
using System.Security.Cryptography;

namespace Mkx.Templates.Sdk.Shared.Utilities;

public static class SecurityHelper
{
    public static string GenerateSecret(int length = 32)
    {
        var randomNumber = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }
}
