using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Mkx.Templates.Application.Services;
using Mkx.Templates.Domain.Notifications;
using Mkx.Templates.Infrastructure;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Shared.DTOs.Push;
using Mkx.Templates.Shared.DTOs.UserAccounts;

namespace Mkx.Templates.Tests;

public sealed class PushTests
{
    private static readonly Guid AccountId = Guid.Parse("0195d5ee-0961-7000-9000-000000000001");

    [Theory]
    [InlineData("http://fcm.googleapis.com/send/1")]
    [InlineData("https://localhost/send/1")]
    [InlineData("https://127.0.0.1/send/1")]
    [InlineData("https://evil.example/send/1")]
    [InlineData("https://fcm.googleapis.com.evil.example/send/1")]
    [InlineData("https://fcm.googleapis.com:8443/send/1")]
    public void RejectsUnsafePushEndpoints(string endpoint)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        using var host = new TestHost();
        using var scope = host.Services.CreateScope();
        var service = new PushSubscriptionService(scope.ServiceProvider.GetRequiredService<AppDbContext>(), config);
        Assert.False(service.IsAllowedEndpoint(endpoint));
        Assert.True(service.IsAllowedEndpoint("https://fcm.googleapis.com/fcm/send/example"));
        Assert.True(service.IsAllowedEndpoint("https://updates.push.services.mozilla.com/wpush/v2/example"));
        Assert.True(service.IsAllowedEndpoint("https://web.push.apple.com/example"));
    }

    [Fact]
    public async Task AnonymousCannotInspectOrModifyPushSubscriptions()
    {
        using var host = new TestHost();
        using var client = host.Client();
        using var configuration = await client.GetAsync("/api/push/configuration", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, configuration.StatusCode);
        using var registered = await client.PostAsJsonAsync("/api/push/subscriptions",
            new RegisterPushSubscriptionRequest("https://fcm.googleapis.com/fcm/send/anon", "key", "auth", null),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, registered.StatusCode);
    }

    [Fact]
    public async Task RegistrationRebindAndOwnerRevocationRemainIsolated()
    {
        using var host = new TestHost();
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var first = new AppUser("push owner", "push-owner") { Id = AccountId };
        var second = new AppUser("other owner", "push-other");
        Assert.True((await users.CreateAsync(first)).Succeeded);
        Assert.True((await users.CreateAsync(second)).Succeeded);

        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Push:Enabled"] = "true",
            ["Push:Subject"] = "mailto:test@example.org",
            ["Push:PublicKey"] = "public",
            ["Push:PrivateKey"] = "private"
        }).Build();
        var service = new PushSubscriptionService(db, cfg);
        var endpoint = "https://fcm.googleapis.com/fcm/send/owner";
        var request = new RegisterPushSubscriptionRequest(endpoint, "p256dh", "auth", "Chrome");
        await service.RegisterAsync(first.Id, request, TestContext.Current.CancellationToken);
        await service.RegisterAsync(second.Id, request with { DeviceName = "Edge" }, TestContext.Current.CancellationToken);
        Assert.Equal(second.Id, (await db.UserPushSubscriptions.SingleAsync(TestContext.Current.CancellationToken)).UserId);
        await service.RevokeAsync(first.Id, endpoint, TestContext.Current.CancellationToken);
        Assert.Single(await db.UserPushSubscriptions.ToListAsync(TestContext.Current.CancellationToken));
        await service.RevokeAsync(second.Id, endpoint, TestContext.Current.CancellationToken);
        Assert.Empty(await db.UserPushSubscriptions.ToListAsync(TestContext.Current.CancellationToken));
    }
}