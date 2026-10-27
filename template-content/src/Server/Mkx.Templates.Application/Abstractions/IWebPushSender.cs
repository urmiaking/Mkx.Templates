using Mkx.Templates.Shared.DTOs.Push;

namespace Mkx.Templates.Application.Abstractions;

/// <summary>Server-side only: send Push notifications to a previously subscribed user.</summary>
public interface IWebPushSender
{
    Task<PushSendResult> SendToUserAsync(Guid userId, PushMessage message, CancellationToken ct = default);
}

public sealed record PushSendResult(int Accepted, int Revoked, int Rejected, int Unknown);
