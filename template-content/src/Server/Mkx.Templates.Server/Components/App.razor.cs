using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Server.Components;

public partial class App
{
    [CascadingParameter] private HttpContext HttpContext { get; set; } = default!;

    private IComponentRenderMode? RenderModeForPage =>
        HttpContext.Request.Path.StartsWithSegments(ClientRoutes.Accounts.Prefix)
        ? null
        : RenderMode.InteractiveAuto;
}
