using Microsoft.AspNetCore.Components;
namespace Mkx.Templates.Client.Layout.Components;

public partial class NotificationsDrawer
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

    private Task Close() => IsOpenChanged.InvokeAsync(false);

    private Task HandleKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs args) =>
        args.Key == "Escape" ? Close() : Task.CompletedTask;
}
