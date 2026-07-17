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
        : new InteractiveWebAssemblyRenderMode(prerender: false);

    /// <summary>
    /// Shows the loading splash screen only when prerendering is disabled.
    /// This ensures reversibility: switching back to InteractiveAutoRenderMode(true)
    /// automatically hides the splash without any other code change.
    /// </summary>
    private bool ShowSplash => RenderModeForPage switch
    {
        InteractiveWebAssemblyRenderMode { Prerender: false } => true,
        InteractiveAutoRenderMode { Prerender: false } => true,
        _ => false
    };
}