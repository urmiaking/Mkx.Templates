using Mkx.Templates.Application.Extensions;
using Mkx.Templates.Client.Extensions;
using Mkx.Templates.Infrastructure.Extensions;
using Mkx.Templates.Sdk.Server.Api.Middlewares;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Server.Components;
using Mkx.Templates.Server.Middlewares;
using Mkx.Templates.Shared.Routes;
using Serilog.Ui.Web.Extensions;
using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;


namespace Mkx.Templates.Server.Extensions;

public static class WebHostingExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplication ConfigureServices()
        {
            var configuration = builder.Configuration;

            builder.Services
                .AddClientServerServices()
                .AddServer(configuration)
                .AddApplication(configuration)
                .AddInfrastructure(configuration);

            return builder.Build();
        }
    }

    extension(WebApplication app)
    {
        public WebApplication ConfigurePipeline()
        {
            app.UseForwardedHeaders();
            if (!app.Environment.IsDevelopment()) app.UseHsts();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
                if (context.Request.Path.StartsWithSegments("/api")) context.Response.Headers.CacheControl = "no-store";
                await next(context);
            });
            Assembly[] additionalAssemblies = [typeof(Mkx.Templates.Client.Routes).Assembly];

            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseHttpsRedirection();
            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"),
                branch => branch.UseStatusCodePagesWithReExecute(ClientRoutes.General.NotFound));
            app.UseRouting();

            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseAuthorization();

            app.UseMiddleware<PathRoleAuthorizationMiddleware>(ClientRoutes.Logs.Base, BuiltinRoles.Administrators);

            app.UseAntiforgery();

            app.MapStaticAssets();

            app.MapServiceWorker();
            app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

            if (app.Configuration.GetValue<bool>("Logging:UseSqlStore")) app.UseSerilogUi();

            app.MapControllers();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(additionalAssemblies);

            return app;
        }

        public WebApplication MapServiceWorker()
        {
            app.MapGet("/service-worker.js", async (HttpContext context, IWebHostEnvironment env) =>
            {
                var filePath = Path.Combine(env.WebRootPath, "service-worker.template.js");
                if (!File.Exists(filePath))
                {
                    return Results.NotFound();
                }

                var content = await File.ReadAllTextAsync(filePath);
                var assemblyPath = typeof(Program).Assembly.Location;
                var buildId = File.GetLastWriteTime(assemblyPath).Ticks.ToString();

                content = content.Replace("__BUILD_ID__", buildId);

                context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                return Results.Text(content, "application/javascript");
            });

            return app;
        }
    }
}
