using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mkx.Templates.Application.Services;
using Mkx.Templates.Sdk.Server.Api;
using Mkx.Templates.Shared.DTOs.Push;
using Mkx.Templates.Sdk.Server.Domain.Identity;

namespace Mkx.Templates.Server.Controllers;

[ApiController]
[Authorize]
[Route("/api/push")]
public sealed class PushController(PushSubscriptionService subscriptions, UserManager<AppUser> users)
    : ApiControllerBase
{
    [HttpGet("configuration")]
    public ActionResult<PushConfigurationResponse> GetConfiguration() => Ok(subscriptions.Configuration());

    [HttpPost("subscriptions")]
    public async Task<IActionResult> Register([FromBody] RegisterPushSubscriptionRequest request, CancellationToken ct)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.Endpoint) || string.IsNullOrWhiteSpace(request.P256dh)
            || string.IsNullOrWhiteSpace(request.Auth)) return BadRequest();
        if (!subscriptions.Configuration().Enabled) return NotFound();
        try { await subscriptions.RegisterAsync(userId, request, ct); }
        catch (ArgumentException) { return BadRequest(); }
        return NoContent();
    }

    [HttpDelete("subscriptions")]
    public async Task<IActionResult> Revoke([FromBody] RevokePushSubscriptionRequest request, CancellationToken ct)
    {
        if (!TryUserId(out var userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.Endpoint)) return BadRequest();
        await subscriptions.RevokeAsync(userId, request.Endpoint, ct);
        return NoContent();
    }

    private bool TryUserId(out Guid id) => Guid.TryParse(users.GetUserId(User), out id);
}
