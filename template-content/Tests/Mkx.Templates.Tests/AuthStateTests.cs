using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Mkx.Templates.Client.Common;
using Mkx.Templates.Client.Extensions;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Shared.Authorization;

namespace Mkx.Templates.Tests;

public class AuthStateTests
{
    [Fact]
    public void ClientRegistersSharedCapabilityDefinitions()
    {
        var services = new ServiceCollection(); services.AddLogging(); services.AddAuthServices();
        using var provider = services.BuildServiceProvider();
        Assert.Contains(provider.GetServices<IApplicationPolicyProvider>(), service => service is AppPolicyProvider);
    }
    [Fact]
    public async Task NetworkFailurePreservesPrincipalButConfirmedLogoutClearsIt()
    {
        using var host = new TestHost(); using var scope = host.Services.CreateScope();
        var handler = new AuthHandler(); using var client = new HttpClient(handler) { BaseAddress = new Uri("https://localhost") };
        await using var provider = new PersistentAuthenticationStateProvider(scope.ServiceProvider.GetRequiredService<PersistentComponentState>(), client, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.True((await provider.GetAuthenticationStateAsync()).User.Identity!.IsAuthenticated);
        handler.Fail = true; await provider.RefreshAsync();
        Assert.True(provider.HasConnectionError); Assert.True((await provider.GetAuthenticationStateAsync()).User.Identity!.IsAuthenticated);
        handler.Fail = false; handler.LoggedOut = true; await provider.RefreshAsync();
        Assert.False(provider.HasConnectionError); Assert.False((await provider.GetAuthenticationStateAsync()).User.Identity!.IsAuthenticated);
    }
    [Fact]
    public async Task BackgroundRefreshKeepsCompletedStateAndDoesNotNotifyForUnchangedClaims()
    {
        using var host = new TestHost(); using var scope = host.Services.CreateScope();
        var handler = new AuthHandler(); using var client = new HttpClient(handler) { BaseAddress = new Uri("https://localhost") };
        await using var provider = new PersistentAuthenticationStateProvider(scope.ServiceProvider.GetRequiredService<PersistentComponentState>(), client, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var original = await provider.GetAuthenticationStateAsync();
        var notifications = new List<Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState>>();
        provider.AuthenticationStateChanged += task => notifications.Add(task);
        handler.Pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var refresh = provider.RefreshAsync();
        Assert.False(refresh.IsCompleted);
        Assert.True(provider.GetAuthenticationStateAsync().IsCompletedSuccessfully);
        Assert.Same(original, await provider.GetAuthenticationStateAsync());
        Assert.Empty(notifications);
        handler.Pending.SetResult();
        await refresh;
        Assert.Empty(notifications);
        handler.ReverseClaims = true;
        await provider.RefreshAsync();
        Assert.Same(original, await provider.GetAuthenticationStateAsync());
        Assert.Empty(notifications);
        handler.ExtraClaim = new Claim("NewCapability", "allowed");
        await provider.RefreshAsync();
        Assert.True(Assert.Single(notifications).IsCompletedSuccessfully);
        Assert.True((await provider.GetAuthenticationStateAsync()).User.HasClaim("NewCapability", "allowed"));
        handler.LoggedOut = true;
        await provider.RefreshAsync();
        Assert.Equal(2, notifications.Count);
        Assert.All(notifications, task => Assert.True(task.IsCompletedSuccessfully));
        Assert.False((await provider.GetAuthenticationStateAsync()).User.Identity!.IsAuthenticated);
    }

    private sealed class AuthHandler : HttpMessageHandler
    {
        public bool Fail { get; set; }
        public bool LoggedOut { get; set; }
        public bool ReverseClaims { get; set; }
        public Claim? ExtraClaim { get; set; }
        public TaskCompletionSource? Pending { get; set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Pending is not null) await Pending.Task.WaitAsync(cancellationToken);
            if (Fail) throw new HttpRequestException("offline");
            var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "0195d5ee-0961-7000-9000-000000000001"), new Claim(ClaimTypes.Name, "test-user")], "Test"));
            if (ExtraClaim is not null) ((ClaimsIdentity)principal.Identity!).AddClaim(ExtraClaim);
            return new HttpResponseMessage(LoggedOut ? HttpStatusCode.Unauthorized : HttpStatusCode.OK) { Content = JsonContent.Create(new UserInfo(ReverseClaims ? principal.Claims.Reverse() : principal.Claims)) };
        }
    }
}
