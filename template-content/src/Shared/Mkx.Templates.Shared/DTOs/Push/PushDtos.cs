using System.ComponentModel.DataAnnotations;

namespace Mkx.Templates.Shared.DTOs.Push;

public sealed record PushConfigurationResponse(bool Enabled, string? PublicKey);
public sealed record RegisterPushSubscriptionRequest(
    [Required, MaxLength(2048)] string Endpoint,
    [Required, MaxLength(256)] string P256dh,
    [Required, MaxLength(128)] string Auth,
    [MaxLength(120)] string? DeviceName);
public sealed record RevokePushSubscriptionRequest([Required, MaxLength(2048)] string Endpoint);
/// <summary>Server-only delivery payload; browser clients cannot choose recipients.</summary>
public sealed record PushMessage(string Title, string Body, string? Url = null, string? Tag = null);
