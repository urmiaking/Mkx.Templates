using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Mkx.Templates.Server.Extensions;

public static class HttpContextExtensions
{
    public static string GetDeviceName(this HttpContext httpContext)
    {
        var ua = httpContext.Request.Headers["User-Agent"].ToString();
        if (string.IsNullOrWhiteSpace(ua))
            return "Unknown Device";

        // Detect Android (extract model)
        var androidMatch = Regex.Match(ua, @"Android.*?;\s*([^;]+?)\s*Build", RegexOptions.IgnoreCase);
        if (androidMatch.Success)
            return androidMatch.Groups[1].Value.Trim();

        // Detect iPhone / iPad
        if (ua.Contains("iPhone"))
            return "iPhone";
        if (ua.Contains("iPad"))
            return "iPad";

        // Detect macOS
        if (ua.Contains("Mac OS X"))
            return "Mac";

        // Detect Windows
        if (ua.Contains("Windows NT"))
            return "Windows PC";

        // Detect Linux
        if (ua.Contains("Linux"))
            return "Linux Device";

        return "Unknown Device";
    }
}
