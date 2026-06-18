using Mkx.Templates.Shared.Routes;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mkx.Templates.Server.Middlewares;

public sealed class PathRoleAuthorizationMiddleware(
    RequestDelegate next,
    string path,
    string requiredRole)
{
    private readonly PathString _path = new(path);

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments(_path))
        {
            if (context.User.Identity?.IsAuthenticated != true ||
                !context.User.IsInRole(requiredRole))
            {
                context.Response.Redirect(ClientRoutes.Accounts.AccessDenied);
                return;
            }
        }

        await next(context);
    }
}
