using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace Mkx.Templates.Server.Services;

/// <summary>Server-side password attempt tracking and single-use visual challenges.</summary>
public sealed class LoginCaptchaService : IDisposable
{
    private const string CookieName = "Mkx.Templates_LoginChallenge";
    private const int Threshold = 3;
    private static readonly TimeSpan FailureLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(5);
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 20000 });
    private readonly Lock _gate = new();

    public static string GetBrowserKey(HttpContext context)
    {
        if (Guid.TryParseExact(context.Request.Cookies[CookieName], "N", out var existing))
            return existing.ToString("N");

        var key = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CookieName, key, new CookieOptions
        {
            HttpOnly = true, Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax, IsEssential = true
        });
        return key;
    }

    public bool IsRequired(string browser, string? account = null)
    {
        lock (_gate)
            return Count("browser:" + browser) >= Threshold ||
                (account is not null && Count("account:" + account) >= Threshold);
    }

    public void RecordFailure(string browser, string account)
    {
        lock (_gate)
        {
            Increment("browser:" + browser);
            Increment("account:" + account);
        }
    }

    public void Reset(string browser, string account)
    {
        lock (_gate)
        {
            _cache.Remove("browser:" + browser);
            _cache.Remove("account:" + account);
            _cache.Remove("challenge:" + browser);
        }
    }

    public CaptchaChallenge Issue(string browser)
    {
        lock (_gate)
        {
            var answer = string.Concat(Enumerable.Range(0, 5).Select(_ =>
                RandomNumberGenerator.GetInt32(2, 10).ToString(CultureInfo.InvariantCulture)));
            var id = Guid.NewGuid().ToString("N");
            _cache.Set("challenge:" + browser, new StoredChallenge(id, answer, DateTimeOffset.UtcNow.Add(ChallengeLifetime)),
                Entry(FailureLifetime));
            return new(id, LoginCaptchaImage.Render(answer));
        }
    }

    public CaptchaChallenge? Refresh(string browser, string? id)
    {
        lock (_gate)
        {
            if (!_cache.TryGetValue<StoredChallenge>("challenge:" + browser, out var challenge) ||
                challenge is null || !string.Equals(challenge.Id, id, StringComparison.Ordinal))
                return null;
            return Issue(browser);
        }
    }

    public bool ValidateAndConsume(string browser, string? id, string? answer)
    {
        lock (_gate)
        {
            var key = "challenge:" + browser;
            if (!_cache.TryGetValue<StoredChallenge>(key, out var challenge) ||
                challenge is null || !string.Equals(challenge.Id, id, StringComparison.Ordinal))
                return false;
            _cache.Remove(key);
            return challenge.ExpiresAt > DateTimeOffset.UtcNow && string.Equals(challenge.Answer, Normalize(answer), StringComparison.Ordinal);
        }
    }

    private int Count(string key) => _cache.Get<int>(key);
    private void Increment(string key) =>
        _cache.Set(key, Math.Min(Threshold, Count(key) + 1), Entry(FailureLifetime));
    private static MemoryCacheEntryOptions Entry(TimeSpan lifetime) =>
        new() { Size = 1, AbsoluteExpirationRelativeToNow = lifetime };

    private static string Normalize(string? answer) => new((answer ?? "").Trim().Select(c => c switch
    {
        >= '۰' and <= '۹' => (char)('0' + c - '۰'),
        >= '٠' and <= '٩' => (char)('0' + c - '٠'),
        _ => c
    }).ToArray());

    public void Dispose() => _cache.Dispose();
    private sealed record StoredChallenge(string Id, string Answer, DateTimeOffset ExpiresAt);
    public sealed record CaptchaChallenge(string Id, string Image);
}
