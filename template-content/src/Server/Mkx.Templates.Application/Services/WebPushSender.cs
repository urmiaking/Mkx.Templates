using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mkx.Templates.Application.Abstractions;
using Mkx.Templates.Domain.Notifications;
using Mkx.Templates.Infrastructure;
using Mkx.Templates.Shared.DTOs.Push;
using WebPush;

namespace Mkx.Templates.Application.Services;

/// <summary>Provider acceptance is not proof of delivery; unknown failures are not blindly retried.</summary>
public sealed class WebPushSender(AppDbContext db, IConfiguration config, PushSubscriptionService subscriptionService, ILogger<WebPushSender> logger) : IWebPushSender
{
    public async Task<PushSendResult> SendToUserAsync(Guid userId, PushMessage message, CancellationToken ct = default)
    {
        if (!subscriptionService.Configuration().Enabled)
            return new(0, 0, 0, 0);

        ArgumentException.ThrowIfNullOrWhiteSpace(message.Title);
        if (message.Title.Length > 200 || message.Body.Length > 1000)
            throw new ArgumentException("Push content is too long.");

        // Prevent notification-click redirects to a different origin.
        var url = message.Url is { Length: > 0 } path &&
                  path.StartsWith('/') && !path.StartsWith("//") &&
                  !path.Contains('\\') && !path.Any(char.IsControl)
            ? path : "/";

        var payload = JsonSerializer.Serialize(new
        {
            title = message.Title,
            body = message.Body,
            url,
            tag = message.Tag is { Length: > 0 and <= 100 } ? message.Tag : null
        });

        var subscriptions = await db.Set<UserPushSubscription>()
            .Where(s => s.UserId == userId).ToListAsync(ct);
        if (subscriptions.Count == 0) return new(0, 0, 0, 0);

        var vapid = new VapidDetails(config["Push:Subject"]!.Trim(),
            config["Push:PublicKey"]!.Trim(), config["Push:PrivateKey"]!.Trim());
        var options = new Dictionary<string, object> { ["vapidDetails"] = vapid, ["TTL"] = 86400 };
        using var client = new WebPushClient();
        int accepted = 0, revoked = 0, rejected = 0, unknown = 0;

        foreach (var entry in subscriptions)
        {
            ct.ThrowIfCancellationRequested();
            if (!subscriptionService.IsAllowedEndpoint(entry.Endpoint))
            {
                db.Set<UserPushSubscription>().Remove(entry);
                revoked++;
                continue;
            }
            try
            {
                await client.SendNotificationAsync(
                    new WebPush.PushSubscription(entry.Endpoint, entry.P256dh, entry.Auth),
                    payload, options, ct);
                accepted++;
            }
            catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
            {
                db.Set<UserPushSubscription>().Remove(entry);
                revoked++;
            }
            catch (WebPushException ex)
            {
                rejected++;
                logger.LogWarning("Push provider rejected endpoint (HTTP {Status}).", (int)ex.StatusCode);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException)
            {
                unknown++;
                logger.LogWarning(ex, "Push delivery outcome is unknown; do not blindly retry.");
            }
        }

        if (revoked > 0) await db.SaveChangesAsync(ct);
        return new(accepted, revoked, rejected, unknown);
    }
}