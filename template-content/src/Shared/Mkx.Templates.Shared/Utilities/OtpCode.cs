namespace Mkx.Templates.Shared.Utilities;

/// <summary>Normalizes pasted, autofilled and typed OTP digits without losing leading zeroes.</summary>
public static class OtpCode
{
    public static string Normalize(string? value) => new((value ?? "").Select(character => character switch
    {
        >= '۰' and <= '۹' => (char)('0' + character - '۰'),
        >= '٠' and <= '٩' => (char)('0' + character - '٠'),
        _ => character
    }).Where(character => character is >= '0' and <= '9').Take(6).ToArray());
}
