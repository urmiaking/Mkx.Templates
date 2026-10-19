using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Mkx.Templates.Application.Abstractions;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Infrastructure;
using Mkx.Templates.Infrastructure.Extensions;
using Mkx.Templates.Shared.Authorization;
#if (IncludeSample)
using Mkx.Templates.Shared.DTOs.SampleItems;
#endif
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.DTOs.Users;
using Mkx.Templates.Shared.Routes;
using Mkx.Templates.Sdk.Server.Shared.Data;

namespace Mkx.Templates.Tests;

public sealed class TestHost : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    internal RecordingSmsSender Sms { get; } = new();
    private readonly bool sqlLogging;
    public TestHost(bool sqlLogging = false) { this.sqlLogging = sqlLogging; connection.Open(); }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Mkx.Templates", "Server=localhost;Database=unused;Integrated Security=true;TrustServerCertificate=true");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Mkx.Templates"] = "Server=localhost;Database=unused;Integrated Security=true;TrustServerCertificate=true",
            ["Database:ApplyMigrationsOnStartup"] = "false",
            ["Database:SeedOnStartup"] = "false",
            ["Logging:UseSqlStore"] = sqlLogging.ToString(),
            ["BootstrapAdmin:Enabled"] = "false"
        }));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>(); services.RemoveAll<DbContextOptions<AppDbContext>>(); services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
            services.RemoveAll<IDataProtectionProvider>();
            services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
            services.RemoveAll<ISmsSender>();
            services.AddSingleton<ISmsSender>(Sms);
            services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "Test"; options.DefaultChallengeScheme = "Test"; options.DefaultForbidScheme = "Test"; }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

        });
    }
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();
        using (var scope = host.Services.CreateScope()) scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        host.Start();
        return host;
    }
    public HttpClient Client(params string[] policies)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        if (policies.Length > 0) client.DefaultRequestHeaders.Add("X-Test-Policies", string.Join(',', policies));
        return client;
    }
    protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) connection.Dispose(); }
}
internal sealed class RecordingSmsSender : ISmsSender
{
    public bool Accepted { get; set; } = true;
    public List<string> Recipients { get; } = [];
    public Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        Recipients.Add(phoneNumber);
        return Task.FromResult(Accepted);
    }
}
public sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Policies", out var header)) return Task.FromResult(AuthenticateResult.NoResult());
        var claims = header.ToString().Split(',').Select(policy => policy.StartsWith("Role:", StringComparison.Ordinal)
            ? new Claim(ClaimTypes.Role, policy[5..]) : new Claim(policy, "")).ToList();
        claims.Add(new(ClaimTypes.NameIdentifier, "0195d5ee-0961-7000-9000-000000000001")); claims.Add(new(ClaimTypes.Name, "test-user"));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")), Scheme.Name)));
    }
}
public class ApiTests
{
    [Fact]
    public void SqlServerMigrationsMatchTheModelWithoutConnectingToADatabase()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Mkx.Templates"] = "Server=localhost;Database=unused;Integrated Security=true;TrustServerCertificate=true"
        }).Build();
        using var services = new ServiceCollection().AddLogging().AddInfrastructure(configuration).BuildServiceProvider();
        using var scope = services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(database.Database.HasPendingModelChanges());
        var sql = database.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>().GenerateScript();
#if (IncludeSample)
        Assert.Contains("CREATE TABLE [Tests]", sql);
#else
        Assert.DoesNotContain("CREATE TABLE [Tests]", sql);
#endif
    }

    [Fact]
    public async Task RecoveryFormPreservesNativeOtpMappingCooldownAndVerification()
    {
        using var host = new TestHost();
        using var client = host.Client();
        await CreatePhoneUserAsync(host);
        var html = await client.GetStringAsync(ClientRoutes.Accounts.ForgotPassword, Token);
        async Task<string> SubmitAsync(int step, string action, string code = "")
        {
            var csrf = System.Text.RegularExpressions.Regex.Match(html, "name=\"__RequestVerificationToken\" value=\"([^\"]+)\"").Groups[1].Value;
            Assert.NotEmpty(csrf);
            using var response = await client.PostAsync(ClientRoutes.Accounts.ForgotPassword, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["_handler"] = "forgotPassword",
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(csrf),
                ["Input.Step"] = step.ToString(),
                ["Input.Action"] = action,
                ["Input.PhoneNumber"] = "09123456789",
                ["Input.OtpCode"] = code
            }), Token);
            html = await response.Content.ReadAsStringAsync(Token);
            Assert.True(response.IsSuccessStatusCode, html);
            return WebUtility.HtmlDecode(html);
        }
        var otpPage = await SubmitAsync(1, "request");
        Assert.Contains("name=\"Input.Step\" value=\"2\"", otpPage);
        Assert.Contains("name=\"Input.OtpCode\"", otpPage);
        Assert.Contains("autocomplete=\"one-time-code\"", otpPage);
        Assert.Equal(6, System.Text.RegularExpressions.Regex.Matches(otpPage, "class=\"sms-otp-digit").Count);
        Assert.Single(host.Sms.Recipients);
        await SubmitAsync(2, "resend");
        Assert.Single(host.Sms.Recipients);
        string code;
        using (var scope = host.Services.CreateScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = await manager.FindByNameAsync("phone-owner");
            code = await manager.GenerateTwoFactorTokenAsync(user!, "Phone");
        }
        // The UI supports Persian digits; the server must accept them too.
        var persianCode = string.Concat(code.Select(digit => (char)('\u06f0' + digit - '0')));
        var passwordPage = await SubmitAsync(2, "verify", persianCode);
        Assert.Contains("name=\"Input.Step\" value=\"3\"", passwordPage);
        Assert.Contains("name=\"Input.NewPassword\"", passwordPage);
        Assert.Contains($"name=\"Input.OtpCode\" value=\"{code}\"", passwordPage);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private static async Task CsrfAsync(HttpClient client) { using var response = await client.GetAsync(ApiUrls.Accounts.Antiforgery(), Token); var body = await response.Content.ReadAsStringAsync(Token); Assert.True(response.IsSuccessStatusCode, body); var token = await response.Content.ReadFromJsonAsync<AntiforgeryResponse>(Token); client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN"); client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token!.RequestToken); }
    private static async Task CreatePhoneUserAsync(TestHost host)
    {
        using var scope = host.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = new AppUser("owner", "phone-owner", phoneNumber: "09123456789")
        {
            Id = Guid.Parse("0195d5ee-0961-7000-9000-000000000001"),
            PhoneNumberConfirmed = true
        };
        Assert.True((await manager.CreateAsync(user)).Succeeded);
        Assert.True((await manager.CreateAsync(new AppUser("other", "other-owner", phoneNumber: "09987654321") { PhoneNumberConfirmed = true })).Succeeded);
    }
    [Fact]
    public async Task PhoneVerificationUsesCurrentUserAndValidatesBeforeSending()
    {
        using var host = new TestHost(); using var client = host.Client("Authenticated"); await CsrfAsync(client); await CreatePhoneUserAsync(host);
        using var foreign = await client.PostAsJsonAsync(ApiUrls.UserAccounts.SendVerificationToken(), new SendVerificationCodeRequest("09987654321", "09111111111"), Token);
        Assert.Equal(HttpStatusCode.BadRequest, foreign.StatusCode); Assert.Empty(host.Sms.Recipients);
        using var invalid = await client.PostAsJsonAsync(ApiUrls.UserAccounts.SendVerificationToken(), new SendVerificationCodeRequest("09123456789", "invalid"), Token);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode); Assert.Empty(host.Sms.Recipients);
        using var sent = await client.PostAsJsonAsync(ApiUrls.UserAccounts.SendVerificationToken(), new SendVerificationCodeRequest("09123456789", "09111111111"), Token);
        Assert.Equal(HttpStatusCode.OK, sent.StatusCode); Assert.Equal("09123456789", Assert.Single(host.Sms.Recipients));
        using var cooldown = await client.PostAsJsonAsync(ApiUrls.UserAccounts.SendVerificationToken(), new SendVerificationCodeRequest("09123456789", "09122222222"), Token);
        Assert.Equal(HttpStatusCode.BadRequest, cooldown.StatusCode); Assert.Single(host.Sms.Recipients);
    }
    [Fact]
    public async Task SmsFailureDoesNotStartSuccessfulSendCooldown()
    {
        using var host = new TestHost(); using var client = host.Client("Authenticated"); await CsrfAsync(client); await CreatePhoneUserAsync(host);
        host.Sms.Accepted = false;
        using var failed = await client.PostAsJsonAsync(ApiUrls.UserAccounts.SendVerificationToken(), new SendVerificationCodeRequest("09123456789", "09111111111"), Token);
        Assert.Equal(HttpStatusCode.BadRequest, failed.StatusCode);
        host.Sms.Accepted = true;
        using var retry = await client.PostAsJsonAsync(ApiUrls.UserAccounts.SendVerificationToken(), new SendVerificationCodeRequest("09123456789", "09111111111"), Token);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
    }
    [Fact]
    public async Task DynamicHtmlDoesNotReceivePublicStaticCacheHeaders()
    {
        using var host = new TestHost(); using var client = host.Client();
        using var page = await client.GetAsync("/", Token);
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.False(page.Headers.CacheControl?.Public == true);
    }
    [Theory]
    [InlineData("/Account/Login")]
    [InlineData("/Account/ForgotPassword")]
    public async Task AccountPagesRenderUsingServerAuthenticationProvider(string path)
    {
        using var host = new TestHost(); using var client = host.Client();
        using var response = await client.GetAsync(path, Token);
        var html = await response.Content.ReadAsStringAsync(Token);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{response.StatusCode} {response.Headers.Location}\n{html}");
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<form", html);
        Assert.Contains("__RequestVerificationToken", html);
        if (path.EndsWith("/Login", StringComparison.Ordinal))
        {
            Assert.Contains("name=\"Input.Username\"", html);
            Assert.Contains("name=\"Input.Password\"", html);
            Assert.Contains("name=\"Input.RememberMe\"", html);
            Assert.Contains("autocomplete=\"username\"", html);
        }
        else Assert.Contains("name=\"Input.PhoneNumber\"", html);
    }
    [Fact]
    public async Task AdministratorCanOpenSqlLogUi()
    {
        using var host = new TestHost(sqlLogging: true);
        using var client = host.Client($"Role:{Mkx.Templates.Sdk.Server.Shared.Authorization.BuiltinRoles.Administrators}");
        using var entry = await client.GetAsync(ClientRoutes.Logs.Base, Token);
        Assert.Equal(HttpStatusCode.MovedPermanently, entry.StatusCode);
        Assert.EndsWith("/serilog-ui/", entry.Headers.Location!.OriginalString);
        using var response = await client.GetAsync(entry.Headers.Location, Token);
        var html = await response.Content.ReadAsStringAsync(Token);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"{response.StatusCode} {response.Headers.Location}\n{html}");
        Assert.Contains("text/html", response.Content.Headers.ContentType!.MediaType!);
        Assert.Contains("Serilog", html, StringComparison.OrdinalIgnoreCase);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LogUiRejectsAnonymousAndNonAdministrator(bool authenticated)
    {
        using var host = new TestHost(sqlLogging: true);
        using var client = authenticated ? host.Client("Unrelated") : host.Client();
        using var response = await client.GetAsync("/serilog-ui/index.html", Token);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(ClientRoutes.Accounts.AccessDenied, response.Headers.Location?.OriginalString);
    }
#if (IncludeSample)
    [Fact] public async Task AnonymousApiReturns401WithoutHtmlRedirect() { using var host = new TestHost(); using var client = host.Client(); using var response = await client.GetAsync(ApiUrls.SampleItems.List(new()), Token); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); Assert.Null(response.Headers.Location); }
    [Fact] public async Task AuthenticatedUserWithoutPolicyGets403() { using var host = new TestHost(); using var client = host.Client("Unrelated"); using var response = await client.GetAsync(ApiUrls.SampleItems.List(new()), Token); Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); }
    [Fact] public async Task ReadPolicyDoesNotGrantWrite() { using var host = new TestHost(); using var client = host.Client(AppPolicies.SampleItems.View); await CsrfAsync(client); using var response = await client.PostAsJsonAsync(ApiUrls.SampleItems.Create(), new CreateSampleItemRequest("name", null), Token); Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); }
    [Fact] public async Task MutationRequiresAntiforgeryToken() { using var host = new TestHost(); using var client = host.Client(AppPolicies.SampleItems.View, AppPolicies.SampleItems.Manage); using var response = await client.PostAsJsonAsync(ApiUrls.SampleItems.Create(), new CreateSampleItemRequest("name", null), Token); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); }
    [Fact]
    public async Task CrudPagingAndConcurrencyAreRealDatabaseOperations()
    {
        using var host = new TestHost(); using var client = host.Client(AppPolicies.SampleItems.View, AppPolicies.SampleItems.Manage); await CsrfAsync(client);
        using var created = await client.PostAsJsonAsync(ApiUrls.SampleItems.Create(), new CreateSampleItemRequest("alpha", "description"), Token); Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = (await created.Content.ReadFromJsonAsync<GetSampleItemResponse>(Token))!;
        var page = (await client.GetFromJsonAsync<PagedList<GetSampleItemResponse>>(ApiUrls.SampleItems.List(new(Take: 1, Search: "alpha")), Token))!; Assert.Equal(1, page.Total); Assert.Single(page.Data);
        using var updated = await client.PutAsJsonAsync(ApiUrls.SampleItems.Get(item.Id), new UpdateSampleItemRequest("beta", null, item.Version), Token); Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var next = (await updated.Content.ReadFromJsonAsync<GetSampleItemResponse>(Token))!; Assert.NotEqual(item.Version, next.Version);
        using var stale = await client.PutAsJsonAsync(ApiUrls.SampleItems.Get(item.Id), new UpdateSampleItemRequest("stale", null, item.Version), Token); Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode); Assert.Equal("application/problem+json", stale.Content.Headers.ContentType!.MediaType);
        using var deleted = await client.DeleteAsync(ApiUrls.SampleItems.Delete(item.Id, next.Version), Token); Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var missing = await client.GetAsync(ApiUrls.SampleItems.Get(item.Id), Token); Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }
    [Fact]
    public async Task ValidationReturnsFieldErrorsAndTraceId()
    {
        using var host = new TestHost(); using var client = host.Client(AppPolicies.SampleItems.View, AppPolicies.SampleItems.Manage); await CsrfAsync(client);
        using var response = await client.PostAsJsonAsync(ApiUrls.SampleItems.Create(), new CreateSampleItemRequest("", null), Token); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync(Token); Assert.Contains("errors", json); Assert.Contains("Name", json); Assert.Contains("traceId", json);
    }
#endif
    [Fact] public async Task HealthAndAuthStateAreNoStore() { using var host = new TestHost(); using var client = host.Client(); using var health = await client.GetAsync("/health/live", Token); Assert.Equal(HttpStatusCode.OK, health.StatusCode); using var auth = await client.GetAsync(ApiUrls.Accounts.AuthState(), Token); Assert.True(auth.Headers.CacheControl!.NoStore); }
    [Fact]
    public async Task IdentityPasswordFailureRollsBackAnEarlierProfileUpdate()
    {
        using var host = new TestHost(); using var client = host.Client(AppPolicies.Users.View, AppPolicies.Users.Manage); await CsrfAsync(client);
        using var created = await client.PostAsJsonAsync(ApiUrls.UserManagement.CreateUser(), new CreateUserDto { Name = "original", UserName = "rollback-test", Password = "safe-long-password", Roles = ["Users"] }, Token);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var page = await client.GetFromJsonAsync<PagedList<UserDto>>(ApiUrls.UserManagement.GetUsers(new(Search: "rollback-test")), Token);
        var user = Assert.Single(page!.Data);
        using var failed = await client.PutAsJsonAsync(ApiUrls.UserManagement.UpdateUser(), new UpdateUserDto { Id = user.Id, Name = "should-roll-back", NewPassword = "aaaaaaaaaaaa", Roles = ["Users"] }, Token);
        Assert.Equal(HttpStatusCode.BadRequest, failed.StatusCode);
        var after = await client.GetFromJsonAsync<UserDto>(ApiUrls.UserManagement.GetUserById(user.Id), Token);
        Assert.Equal("original", after!.Name);
    }
    [Fact]
    public async Task UnknownRoleIsRejectedWithoutCreatingAUser()
    {
        using var host = new TestHost(); using var client = host.Client(AppPolicies.Users.View, AppPolicies.Users.Manage); await CsrfAsync(client);
        using var invalid = await client.PostAsJsonAsync(ApiUrls.UserManagement.CreateUser(), new CreateUserDto { Name = "name", UserName = "invalid-role", Password = "safe-long-password", Roles = ["not-a-builtin-role"] }, Token);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var page = await client.GetFromJsonAsync<PagedList<UserDto>>(ApiUrls.UserManagement.GetUsers(new(Search: "invalid-role")), Token); Assert.Equal(0, page!.Total);
    }
}
