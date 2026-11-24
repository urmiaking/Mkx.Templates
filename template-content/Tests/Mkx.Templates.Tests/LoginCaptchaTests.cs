using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Server.Services;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Tests;

public sealed class LoginCaptchaTests
{
    private const string CorrectPassword = "Test-password-1234";
    private const string UserName = "captcha-owner";
    private const string Phone = "09123456789";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public void ChallengeIsBrowserBoundSingleUseAndSupportsLocalizedDigits()
    {
        using var service = new LoginCaptchaService();
        for (var i = 0; i < 2; i++) service.RecordFailure("browser", "account");
        Assert.False(service.IsRequired("browser", "account"));
        service.RecordFailure("browser", "account");
        Assert.True(service.IsRequired("browser", "other"));
        Assert.True(service.IsRequired("other", "account"));
        Assert.False(service.IsRequired("other", "other"));
        var challenge = service.Issue("browser");
        Assert.StartsWith("data:image/png;base64,", challenge.Image);
        var png = Convert.FromBase64String(challenge.Image.Split(',')[1]);
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, png[..8]);
        var answer = ReadAnswer(service, "browser");
        Assert.False(service.ValidateAndConsume("other", challenge.Id, answer));
        var persian = string.Concat(answer.Select(c => (char)('۰' + c - '0')));
        Assert.True(service.ValidateAndConsume("browser", challenge.Id, persian));
        Assert.False(service.ValidateAndConsume("browser", challenge.Id, answer));
        var refreshed = service.Issue("browser");
        Assert.False(service.ValidateAndConsume("browser", refreshed.Id, "00000"));
        Assert.False(service.ValidateAndConsume("browser", refreshed.Id, ReadAnswerOrEmpty(service, "browser")));
        service.Reset("browser", "account");
        Assert.False(service.IsRequired("browser", "account"));
    }

    [Fact]
    public async Task ThirdFailureRequiresCaptchaBeforePasswordAndRefreshDoesNotCount()
    {
        using var host = new TestHost();
        using var client = host.Client();
        using (var scope = host.Services.CreateScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var user = new AppUser("captcha owner", UserName, phoneNumber: Phone) { PhoneNumberConfirmed = true };
            Assert.True((await manager.CreateAsync(user, CorrectPassword)).Succeeded);
        }

        using var initial = await client.GetAsync(ClientRoutes.Accounts.Login, Token);
        Assert.Equal(HttpStatusCode.OK, initial.StatusCode);
        var browser = Regex.Match(string.Join(";", initial.Headers.GetValues("Set-Cookie")),
            Regex.Escape("Mkx.Templates_LoginChallenge") + "=([a-f0-9]{32})").Groups[1].Value;
        Assert.NotEmpty(browser);
        var html = await initial.Content.ReadAsStringAsync(Token);
        Assert.DoesNotContain("name=\"Input.CaptchaAnswer\"", html);

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            using var response = await Submit(client, html, attempt == 2 ? Phone : UserName, "invalid-password");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            html = await response.Content.ReadAsStringAsync(Token);
            Assert.Contains("خطا: اطلاعات ورودی معتبر نیست", WebUtility.HtmlDecode(html));
            Assert.Equal(attempt >= 3, html.Contains("name=\"Input.CaptchaAnswer\"", StringComparison.Ordinal));
        }
        Assert.Equal(3, await FailureCount(host));

        // A new browser still cannot bypass the per-account threshold.
        using var other = host.Client();
        var otherHtml = await other.GetStringAsync(ClientRoutes.Accounts.Login, Token);
        using var missing = await Submit(other, otherHtml, UserName, CorrectPassword);
        Assert.Equal(HttpStatusCode.OK, missing.StatusCode);
        Assert.Contains("name=\"Input.CaptchaAnswer\"", await missing.Content.ReadAsStringAsync(Token));
        Assert.Equal(3, await FailureCount(host));

        var firstId = Field(html, "Input.CaptchaId");
        using var noToken = await client.PostAsync(ApiUrls.Accounts.RefreshCaptcha(), new FormUrlEncodedContent(new Dictionary<string, string> { ["id"] = firstId }), Token);
        Assert.Equal(HttpStatusCode.BadRequest, noToken.StatusCode);
        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrls.Accounts.RefreshCaptcha())
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["id"] = firstId })
        };
        refreshRequest.Headers.Add("X-CSRF-TOKEN", Field(html, "__RequestVerificationToken"));
        using var refresh = await client.SendAsync(refreshRequest, Token);
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.True(refresh.Headers.CacheControl?.NoStore);
        var challenge = await refresh.Content.ReadFromJsonAsync<LoginCaptchaService.CaptchaChallenge>(Token);
        Assert.NotNull(challenge);
        var refreshedId = challenge.Id;
        Assert.StartsWith("data:image/png;base64,", challenge.Image);
        Assert.NotEqual(firstId, refreshedId);
        Assert.Equal(3, await FailureCount(host));

        using var wrong = await Submit(client, html, UserName, CorrectPassword, refreshedId, "00000");
        Assert.Equal(HttpStatusCode.OK, wrong.StatusCode);
        html = await wrong.Content.ReadAsStringAsync(Token);
        Assert.NotEqual(refreshedId, Field(html, "Input.CaptchaId"));
        Assert.Contains("کد امنیتی نادرست", WebUtility.HtmlDecode(html));
        Assert.Contains("data-login-error", html);
        Assert.Equal(3, await FailureCount(host));

        var captcha = host.Services.GetRequiredService<LoginCaptchaService>();
        var answer = ReadAnswer(captcha, browser);
        using var wrongPassword = await Submit(client, html, UserName, "invalid-password", Field(html, "Input.CaptchaId"), answer);
        Assert.Equal(HttpStatusCode.OK, wrongPassword.StatusCode);
        html = await wrongPassword.Content.ReadAsStringAsync(Token);
        Assert.Contains("خطا: اطلاعات ورودی معتبر نیست", WebUtility.HtmlDecode(html));
        Assert.Equal(4, await FailureCount(host));
        answer = ReadAnswer(captcha, browser);
        using var success = await Submit(client, html, UserName, CorrectPassword, Field(html, "Input.CaptchaId"), answer);
        Assert.Equal(HttpStatusCode.Redirect, success.StatusCode);
        Assert.False(captcha.IsRequired(browser, UserName.ToUpperInvariant()));
        Assert.Equal(0, await FailureCount(host));
    }

    private static async Task<int> FailureCount(TestHost host)
    {
        using var scope = host.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await manager.FindByNameAsync(UserName);
        return await manager.GetAccessFailedCountAsync(user!);
    }

    private static string Field(string html, string name)
    {
        var match = Regex.Match(html, "name=\"" + Regex.Escape(name) + "\" value=\"([^\"]+)\"");
        Assert.True(match.Success, name);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static Task<HttpResponseMessage> Submit(HttpClient client, string html, string username,
        string password, string? id = null, string? answer = null)
    {
        var values = new Dictionary<string, string>
        {
            ["_handler"] = "login", ["__RequestVerificationToken"] = Field(html, "__RequestVerificationToken"),
            ["Input.Username"] = username, ["Input.Password"] = password
        };
        if (id is not null) values["Input.CaptchaId"] = id;
        if (answer is not null) values["Input.CaptchaAnswer"] = answer;
        return client.PostAsync(ClientRoutes.Accounts.Login, new FormUrlEncodedContent(values), Token);
    }

    // Test-only inspection of server state; the public challenge never contains the answer.
    private static string ReadAnswer(LoginCaptchaService service, string browser)
    {
        var answer = ReadAnswerOrEmpty(service, browser);
        Assert.Matches("^[2-9]{5}$", answer);
        return answer;
    }

    private static string ReadAnswerOrEmpty(LoginCaptchaService service, string browser)
    {
        var cache = (IMemoryCache)typeof(LoginCaptchaService).GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(service)!;
        var stored = cache.Get("challenge:" + browser);
        return (string?)stored?.GetType().GetProperty("Answer")?.GetValue(stored) ?? "";
    }
}
