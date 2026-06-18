using Mkx.Templates.Application.Extensions;
using Mkx.Templates.Client.Extensions;
using Mkx.Templates.Server.Components;
using Mkx.Templates.Infrastructure.Extensions;
using Mkx.Templates.Sdk.Server.Api.Middlewares;
using Mkx.Templates.Server.Middlewares;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Shared.Routes;
using System.Reflection;


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
            Assembly[] additionalAssemblies = [typeof(Mkx.Templates.Client.Routes).Assembly];

            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStaticFileCache();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<PathRoleAuthorizationMiddleware>(ClientRoutes.Logs.Base, BuiltinRoles.Administrators);

            app.UseAntiforgery();

            app.MapStaticAssets();

            app.MapControllers();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(additionalAssemblies);

            return app;
        }
    }
}
