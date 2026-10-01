using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Tests;
using MudBlazor;

namespace Mkx.Templates.Client.Components.Tests;

public partial class TestEditor
{
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;
    [Parameter] public GetTestResponse? Item { get; set; }
    private MudForm _form = default!;
    private string _name = "";
    private string? _description;
    private bool _dirty;
    protected override async Task OnInitializedAsync()
    {
        _name = Item?.Name ?? ""; _description = Item?.Description;
        await base.OnInitializedAsync();
    }
    private void NameChanged(string value) { _name = value; _dirty = true; }
    private void DescriptionChanged(string? value) { _description = value; _dirty = true; }
    private string? FieldError(string field) => ValidationErrors.TryGetValue(field, out var errors) ? string.Join(" ", errors) : null;
    private Task<bool?> ConfirmDiscardAsync() => DialogService.ShowMessageBoxAsync("تغییرات ذخیره نشده", "تغییرات کنار گذاشته شود؟", yesText: "کنار گذاشتن", cancelText: "ادامه ویرایش");
    private async Task CancelAsync() { if (!_dirty || await ConfirmDiscardAsync() == true) { _dirty = false; Dialog.Cancel(); } }
    private async Task ConfirmNavigationAsync(LocationChangingContext context) { if (_dirty && await ConfirmDiscardAsync() != true) context.PreventNavigation(); }
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        await _form.ValidateAsync();
        if (!_form.IsValid) return;
        var result = await TryRequestAsync<ITestService, GetTestResponse>((service, token) => Item is null
            ? service.CreateAsync(new(_name, _description), token)
            : service.UpdateAsync(Item.Id, new(_name, _description, Item.Version), token));
        if (result.Succeeded) { _dirty = false; AddSuccessToast("ذخیره شد."); Dialog.Close(DialogResult.Ok(true)); }
    }
}
