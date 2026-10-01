using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Mkx.Templates.Application.Abstractions;
using Mkx.Templates.Application.Services;
using Mkx.Templates.Server.Services;

namespace Mkx.Templates.Server.Extensions;

public static class HostingSecurityExtensions
{
    public static IServiceCollection AddHostingSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
        services.AddHttpClient<ISmsSender, SmsSender>(client => client.Timeout = TimeSpan.FromSeconds(15));
        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var proxy in configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
                options.KnownProxies.Add(IPAddress.Parse(proxy));
        });
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var auth = context.Request.Path.StartsWithSegments("/Account") || context.Request.Path.StartsWithSegments("/api/Account")
                    || context.Request.Path.Value?.Contains("verification", StringComparison.OrdinalIgnoreCase) == true;
                var safe = HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
                if (safe) return RateLimitPartition.GetNoLimiter("read");
                var key = $"{(auth ? "auth" : "write")}:{context.Connection.RemoteIpAddress}";
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = auth ? 20 : 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await Results.Problem(statusCode: 429, title: "Too many requests", detail: "Wait a minute before trying again.")
                    .ExecuteAsync(context.HttpContext);
            };
        });
        return services;
    }
}
