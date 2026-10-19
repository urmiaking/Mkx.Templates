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
        await FetchAsync(_lifetime.Token, notify: true);
    }

    private async Task<AuthenticationState> FetchAsync(CancellationToken token, bool notify = false)
    {
        await _gate.WaitAsync(token);
        try
        {
            var previous = _current;
            var hadConnectionError = HasConnectionError;
            try
            {
                using var response = await _client.GetAsync(ApiUrls.Accounts.AuthState(), token);
                AuthenticationState next;
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    next = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                else
                {
                    response.EnsureSuccessStatusCode();
                    var info = await response.Content.ReadFromJsonAsync<UserInfo>(_jsonOptions, token)
                        ?? throw new JsonException("Missing authentication state.");
                    next = CreateState(info);
                }
                if (!SamePrincipal(_current, next)) _current = next;
                HasConnectionError = false;
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
            {
                if (token.IsCancellationRequested) throw;
                HasConnectionError = true; // Retain the last known principal; network failure is not logout.
            }
            if (notify && (!ReferenceEquals(previous, _current) || hadConnectionError != HasConnectionError))
            {
                _stateTask = Task.FromResult(_current);
                NotifyAuthenticationStateChanged(_stateTask);
            }
            return _current;
        }
        finally { _gate.Release(); }
    }

    private static bool SamePrincipal(AuthenticationState left, AuthenticationState right) =>
        left.User.Identity?.IsAuthenticated == right.User.Identity?.IsAuthenticated &&
        left.User.Claims.Select(claim => (claim.Type, claim.Value)).OrderBy(claim => claim.Type, StringComparer.Ordinal)
            .ThenBy(claim => claim.Value, StringComparer.Ordinal).SequenceEqual(
                right.User.Claims.Select(claim => (claim.Type, claim.Value)).OrderBy(claim => claim.Type, StringComparer.Ordinal)
                    .ThenBy(claim => claim.Value, StringComparer.Ordinal));

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
