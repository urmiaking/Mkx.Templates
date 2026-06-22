using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Mkx.Templates.Client.Components;
using Mkx.Templates.Client.Services;
using MudBlazor;

namespace Mkx.Templates.Client.Layout.Components;

public partial class NotificationsDrawer : AppComponentBase
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Inject] public ThemeService ThemeService { get; set; } = default!;

    protected List<NotificationItem> Notifications { get; set; } = [];

    protected override void OnInitialized()
    {
        ThemeService.OnToggleMode += OnThemeToggled;
        ThemeService.OnPaletteChanged += OnPaletteChanged;

        // Initialize demo notifications
        Notifications =
        [
            new()
            {
                Id = 1,
                Title = "درخواست مرخصی تأیید شد",
                Message = "درخواست مرخصی شما برای تاریخ ۱۴۰۵/۰۴/۰۱ توسط مدیر تأیید گردید.",
                Time = "۱۰ دقیقه پیش",
                Icon = Icons.Material.Filled.CheckCircle,
                Color = Color.Success,
                IsRead = false
            },
            new()
            {
                Id = 2,
                Title = "ورود جدید به سیستم",
                Message = "یک دستگاه جدید با سیستم‌عامل ویندوز و مرورگر کروم وارد حساب کاربری شما شد.",
                Time = "۲ ساعت پیش",
                Icon = Icons.Material.Filled.Security,
                Color = Color.Warning,
                IsRead = false
            },
            new()
            {
                Id = 3,
                Title = "بروزرسانی نسخه جدید",
                Message = "بروزرسانی نسخه 2.5.0 امشب ساعت 02:00 اعمال می‌شود. لطفاً کارهای خود را ذخیره کنید.",
                Time = "دیروز",
                Icon = Icons.Material.Filled.Update,
                Color = Color.Info,
                IsRead = true
            }
        ];
    }

    private void OnThemeToggled(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnPaletteChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task CloseDrawer()
    {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
    }

    private void ToggleRead(NotificationItem item)
    {
        if (item.SwipeOffset < 5)
        {
            item.IsRead = true;
            StateHasChanged();
        }
    }

    // --- SWIPE TO DISMISS LOGIC ---

    protected double GetOpacity(NotificationItem item)
    {
        if (item.IsDismissing) return 0.0;
        if (item.SwipeOffset == 0) return 1.0;
        var opacity = 1.0 - (item.SwipeOffset / 300.0);
        return Math.Max(0.0, Math.Min(1.0, opacity));
    }

    // Mouse handlers
    protected void HandleMouseDown(MouseEventArgs e, NotificationItem item)
    {
        item.IsSwiping = true;
        item.StartX = e.ClientX;
        item.SwipeOffset = 0;
    }

    protected void HandleMouseMove(MouseEventArgs e, NotificationItem item)
    {
        if (!item.IsSwiping) return;

        var diff = e.ClientX - item.StartX;
        if (diff > 0)
        {
            item.SwipeOffset = diff;
        }
        else
        {
            item.SwipeOffset = 0;
        }
    }

    protected async Task HandleMouseUp(MouseEventArgs e, NotificationItem item)
    {
        if (!item.IsSwiping) return;
        item.IsSwiping = false;

        if (item.SwipeOffset > 100)
        {
            await DismissItem(item);
        }
        else
        {
            item.SwipeOffset = 0;
        }
    }

    protected async Task HandleMouseLeave(MouseEventArgs e, NotificationItem item)
    {
        if (!item.IsSwiping) return;
        item.IsSwiping = false;

        if (item.SwipeOffset > 100)
        {
            await DismissItem(item);
        }
        else
        {
            item.SwipeOffset = 0;
        }
    }

    // Touch handlers
    protected void HandleTouchStart(TouchEventArgs e, NotificationItem item)
    {
        if (e.Touches.Length > 0)
        {
            item.IsSwiping = true;
            item.StartX = e.Touches[0].ClientX;
            item.SwipeOffset = 0;
        }
    }

    protected void HandleTouchMove(TouchEventArgs e, NotificationItem item)
    {
        if (!item.IsSwiping || e.Touches.Length == 0) return;

        var diff = e.Touches[0].ClientX - item.StartX;
        if (diff > 0)
        {
            item.SwipeOffset = diff;
        }
        else
        {
            item.SwipeOffset = 0;
        }
    }

    protected async Task HandleTouchEnd(TouchEventArgs e, NotificationItem item)
    {
        if (!item.IsSwiping) return;
        item.IsSwiping = false;

        if (item.SwipeOffset > 100)
        {
            await DismissItem(item);
        }
        else
        {
            item.SwipeOffset = 0;
        }
    }

    private async Task DismissItem(NotificationItem item)
    {
        item.IsDismissing = true;
        item.SwipeOffset = 450;
        StateHasChanged();
        await Task.Delay(300);
        Notifications.Remove(item);
        StateHasChanged();
    }

    protected async Task DismissAll()
    {
        for (int i = 0; i < Notifications.Count; i++)
        {
            var item = Notifications[i];
            item.IsDismissing = true;
            item.SwipeOffset = 450;
            StateHasChanged();
            await Task.Delay(100);
        }

        await Task.Delay(300);
        Notifications.Clear();
        StateHasChanged();
    }

    public override async ValueTask DisposeAsync()
    {
        ThemeService.OnToggleMode -= OnThemeToggled;
        ThemeService.OnPaletteChanged -= OnPaletteChanged;
        await base.DisposeAsync();
    }
}

public class NotificationItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Time { get; set; } = "";
    public string Icon { get; set; } = "";
    public Color Color { get; set; }
    public bool IsRead { get; set; }

    // Swipe & Dismiss State
    public bool IsDismissing { get; set; }
    public double SwipeOffset { get; set; }
    public bool IsSwiping { get; set; }
    public double StartX { get; set; }
}
