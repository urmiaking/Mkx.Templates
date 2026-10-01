using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Client.Common;

public sealed class PersistentAuthenticationStateProvider : AuthenticationStateProvider, IAsyncDisposable
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SemaphoreSlim _gate = new(1);
    private readonly Task _refreshLoop;
    private AuthenticationState _current = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private Task<AuthenticationState> _stateTask;
    public bool HasConnectionError { get; private set; }

    public PersistentAuthenticationStateProvider(PersistentComponentState state, HttpClient client, JsonSerializerOptions jsonOptions)
    {
        _client = client;
        _jsonOptions = jsonOptions;
        if (state.TryTakeFromJson<UserInfo>(nameof(UserInfo), out var userInfo) && userInfo is not null)
        {
            _current = CreateState(userInfo);
            _stateTask = Task.FromResult(_current);
        }
        else _stateTask = FetchAsync(_lifetime.Token);
        _refreshLoop = RefreshLoopAsync();
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _stateTask;

    public async Task RefreshAsync()
    {
        await _stateTask;
        _stateTask = FetchAsync(_lifetime.Token);
        NotifyAuthenticationStateChanged(_stateTask);
        await _stateTask;
    }

    private async Task<AuthenticationState> FetchAsync(CancellationToken token)
    {
        await _gate.WaitAsync(token);
        try
        {
            using var response = await _client.GetAsync(ApiUrls.Accounts.AuthState(), token);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                _current = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            else
            {
                response.EnsureSuccessStatusCode();
                var info = await response.Content.ReadFromJsonAsync<UserInfo>(_jsonOptions, token)
                    ?? throw new JsonException("Missing authentication state.");
                _current = CreateState(info);
            }
            HasConnectionError = false;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
        {
            if (token.IsCancellationRequested) throw;
            HasConnectionError = true; // Retain the last known principal; network failure is not logout.
        }
        finally { _gate.Release(); }
        return _current;
    }

    private static AuthenticationState CreateState(UserInfo info) => new(new ClaimsPrincipal(
        info.UserClaims.Count > 0 ? new ClaimsIdentity(info.Claims, nameof(PersistentAuthenticationStateProvider)) : new ClaimsIdentity()));

    private async Task RefreshLoopAsync()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        try
        {
            while (await timer.WaitForNextTickAsync(_lifetime.Token)) await RefreshAsync();
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
    }

    public async ValueTask DisposeAsync()
    {
        await _lifetime.CancelAsync();
        try { await _stateTask; } catch (OperationCanceledException) { }
        await _refreshLoop;
        _lifetime.Dispose();
    }
}
