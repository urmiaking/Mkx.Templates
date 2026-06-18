using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Mkx.Templates.Server.Middlewares;

public sealed class StaticFileCacheMiddleware(RequestDelegate next, TimeSpan cacheDuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode == StatusCodes.Status200OK &&
                context.Request.Path.HasValue &&
                !context.Response.Headers.ContainsKey("Cache-Control"))
            {
                context.Response.Headers.Append(
                    "Cache-Control",
                    $"public, max-age={(int)cacheDuration.TotalSeconds}");
            }

            return Task.CompletedTask;
        });

        await next(context);
    }
}

public static class StaticFileCacheMiddlewareExtensions
{
    public static IApplicationBuilder UseStaticFileCache(
        this IApplicationBuilder app,
        int days = 7)
    {
        return app.UseMiddleware<StaticFileCacheMiddleware>(TimeSpan.FromDays(days));
    }
}
