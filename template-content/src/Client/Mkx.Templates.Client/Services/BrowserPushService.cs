using System.Net.Http.Json;
using Microsoft.JSInterop;
using Mkx.Templates.Shared.DTOs.Push;

namespace Mkx.Templates.Client.Services;

/// <summary>Only explicit user interaction requests permission; sign-in restores already-granted subscriptions.</summary>
public sealed class BrowserPushService(HttpClient http, IJSRuntime js)
{
    // Cache the public VAPID key: permission prompts must run before any network await in a user-click handler.
    private PushConfigurationResponse? _cachedConfiguration;

    public async Task<BrowserPushStatus> StatusAsync(CancellationToken ct = default)
    {
        var config = _cachedConfiguration ??= await http.GetFromJsonAsync<PushConfigurationResponse>("/api/push/configuration", ct);
        if (config is not { Enabled: true, PublicKey: { Length: > 0 } })
            return new(false, "unavailable", false, false);
        try
        {
            var state = await js.InvokeAsync<PushState>("mkxPush.state", ct);
            return new(state.Supported, state.Permission, state.Subscribed, true);
        }
        catch (JSException) { return new(false, "unsupported", false, true); }
    }

    public async Task<BrowserPushStatus> EnableAsync(CancellationToken ct = default)
    {
        var config = _cachedConfiguration ??= await http.GetFromJsonAsync<PushConfigurationResponse>("/api/push/configuration", ct);
        if (config is not { Enabled: true, PublicKey: { Length: > 0 } })
            return new(false, "unavailable", false, false);
        return await SubscribeAsync("mkxPush.subscribe", config.PublicKey, ct);
    }

    public Task<BrowserPushStatus> RestoreGrantedAsync(CancellationToken ct = default) =>
        SubscribeAsync("mkxPush.subscribeGranted", ct);

    private async Task<BrowserPushStatus> SubscribeAsync(string method, CancellationToken ct)
    {
        var config = await http.GetFromJsonAsync<PushConfigurationResponse>("/api/push/configuration", ct);
        if (config is not { Enabled: true, PublicKey: { Length: > 0 } })
            return new(false, "unavailable", false, false);
        return await SubscribeAsync(method, config.PublicKey, ct);
    }

    private async Task<BrowserPushStatus> SubscribeAsync(string method, string publicKey, CancellationToken ct)
    {
        var response = await js.InvokeAsync<PushRegistration>(method, ct, publicKey);
        if (response is not { Subscribed: true, Endpoint: not null, P256dh: not null, Auth: not null })
            return new(response.Supported, response.Permission, false, true);
        using var save = await http.PostAsJsonAsync("/api/push/subscriptions",
            new RegisterPushSubscriptionRequest(response.Endpoint, response.P256dh, response.Auth, response.DeviceName), ct);
        save.EnsureSuccessStatusCode();
        return new(true, "granted", true, true);
    }

    public async Task DisableAsync(CancellationToken ct = default)
    {
        var state = await js.InvokeAsync<PushState>("mkxPush.state", ct);
        try
        {
            if (!string.IsNullOrWhiteSpace(state.Endpoint))
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/push/subscriptions")
                {
                    Content = JsonContent.Create(new RevokePushSubscriptionRequest(state.Endpoint))
                };
                using var response = await http.SendAsync(request, ct);
                response.EnsureSuccessStatusCode();
            }
        }
        finally
        {
            // On API failure, local unsubscribe still prevents notifications to this browser.
            await js.InvokeVoidAsync("mkxPush.unsubscribe", ct, true);
        }
    }

    // Called while still authenticated; do not turn a failed logout into a silent disconnect.
    public async Task DetachOnLogoutAsync()
    {
        try { await DisableAsync(); }
        catch (Exception ex) when (ex is HttpRequestException or JSException or JSDisconnectedException or TaskCanceledException)
        {
            try { await js.InvokeVoidAsync("mkxPush.unsubscribe", CancellationToken.None, true); }
            catch (JSException) { }
        }
    }

    private sealed record PushState(bool Supported, string Permission, bool Subscribed, string? Endpoint);
    private sealed record PushRegistration(bool Supported, string Permission, bool Subscribed,
        string? Endpoint, string? P256dh, string? Auth, string? DeviceName);
}

public sealed record BrowserPushStatus(bool Supported, string Permission, bool Subscribed, bool Configured);