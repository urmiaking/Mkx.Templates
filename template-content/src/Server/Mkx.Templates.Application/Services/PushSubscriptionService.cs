using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mkx.Templates.Domain.Notifications;
using Mkx.Templates.Infrastructure;
using Mkx.Templates.Shared.DTOs.Push;

namespace Mkx.Templates.Application.Services;

public sealed class PushSubscriptionService(AppDbContext db, IConfiguration configuration)
{
    public PushConfigurationResponse Configuration()
    {
        var enabled = configuration.GetValue<bool>("Push:Enabled")
            && !string.IsNullOrWhiteSpace(configuration["Push:PublicKey"])
            && !string.IsNullOrWhiteSpace(configuration["Push:PrivateKey"])
            && IsValidSubject(configuration["Push:Subject"]);
        return new(enabled, enabled ? configuration["Push:PublicKey"] : null);
    }

    public static bool IsValidSubject(string? subject) =>
        !string.IsNullOrWhiteSpace(subject) &&
        (subject.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            ? subject.Length > 7
            : Uri.TryCreate(subject, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps);

    // Never send server-side Web Push requests to arbitrary user-supplied URLs (SSRF protection).
    // Operators may extend this list for other trusted browser push providers.
    public bool IsAllowedEndpoint(string endpoint)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps || uri.Port != 443
            || !string.IsNullOrEmpty(uri.UserInfo)
            || System.Net.IPAddress.TryParse(uri.Host, out _))
            return false;

        var allowed = configuration.GetSection("Push:AllowedEndpointHosts").Get<string[]>()
            ?? ["fcm.googleapis.com", ".push.services.mozilla.com", "web.push.apple.com", ".notify.windows.com"];
        return allowed.Any(host => !string.IsNullOrWhiteSpace(host) &&
            (host.StartsWith('.')
                ? uri.Host.EndsWith(host, StringComparison.OrdinalIgnoreCase)
                : uri.Host.Equals(host, StringComparison.OrdinalIgnoreCase)));
    }

    public async Task RegisterAsync(Guid userId, RegisterPushSubscriptionRequest request, CancellationToken ct)
    {
        if (!Configuration().Enabled) throw new InvalidOperationException("Web Push is not configured.");
        if (!IsAllowedEndpoint(request.Endpoint)
            || string.IsNullOrWhiteSpace(request.P256dh) || string.IsNullOrWhiteSpace(request.Auth))
            throw new ArgumentException("Invalid push subscription.");

        var hash = HashEndpoint(request.Endpoint);
        var subscription = await db.Set<UserPushSubscription>().SingleOrDefaultAsync(x => x.EndpointHash == hash, ct);
        if (subscription is null)
        {
            if (await db.Set<UserPushSubscription>().CountAsync(x => x.UserId == userId, ct) >= 20)
                throw new ArgumentException("Maximum number of browser subscriptions reached.");
            subscription = new UserPushSubscription
            {
                UserId = userId, Endpoint = request.Endpoint, EndpointHash = hash,
                P256dh = request.P256dh, Auth = request.Auth
            };
            db.Set<UserPushSubscription>().Add(subscription);
        }
        subscription.UserId = userId;
        subscription.Endpoint = request.Endpoint;
        subscription.P256dh = request.P256dh;
        subscription.Auth = request.Auth;
        subscription.DeviceName = request.DeviceName;
        subscription.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task RevokeAsync(Guid userId, string endpoint, CancellationToken ct)
    {
        var hash = HashEndpoint(endpoint);
        await db.Set<UserPushSubscription>()
            .Where(x => x.UserId == userId && x.EndpointHash == hash)
            .ExecuteDeleteAsync(ct);
    }

    public static string HashEndpoint(string endpoint) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(endpoint)));
}