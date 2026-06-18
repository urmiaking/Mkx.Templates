using Mkx.Templates.Shared.Routes;
using MudBlazor;

namespace Mkx.Templates.Client.Pages;

public partial class Home
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home)
    ];
}