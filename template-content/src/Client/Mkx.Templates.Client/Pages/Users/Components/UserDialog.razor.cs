using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.DTOs.Users;
using MudBlazor;

namespace Mkx.Templates.Client.Pages.Users.Components;

public partial class UserDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public bool IsEdit { get; set; }
    [Parameter] public UserModel Model { get; set; } = new();

    private MudForm _form = default!;

    public class UserModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public List<string> SelectedRoles { get; set; } = [];
    }

    private void ToggleRole(string role, bool isSelected)
    {
        if (isSelected && !Model.SelectedRoles.Contains(role))
        {
            Model.SelectedRoles.Add(role);
        }
        else if (!isSelected && Model.SelectedRoles.Contains(role))
        {
            Model.SelectedRoles.Remove(role);
        }
    }

    private async Task Submit()
    {
        if (IsBusy) return;
        await _form.ValidateAsync();
        if (_form.IsValid)
        {
            var result = await TryRequestAsync<IUserManagementService, bool>((service, ct) => IsEdit
                ? service.UpdateUserAsync(new UpdateUserDto { Id = Model.Id, Name = Model.Name, Email = Model.Email, PhoneNumber = Model.PhoneNumber, NewPassword = Model.Password, Roles = Model.SelectedRoles }, ct)
                : service.CreateUserAsync(new CreateUserDto { Name = Model.Name, UserName = Model.UserName, Email = Model.Email, PhoneNumber = Model.PhoneNumber, Password = Model.Password ?? "", Roles = Model.SelectedRoles }, ct));
            if (result.Succeeded && result.Value) { _originalModel = JsonSerializer.Serialize(Model); AddSuccessToast("ذخیره شد."); MudDialog.Close(DialogResult.Ok(true)); }
        }
    }

    private string _originalModel = "";
    private bool IsDirty => _originalModel.Length > 0 && JsonSerializer.Serialize(Model) != _originalModel;
    protected override async Task OnInitializedAsync()
    {
        _originalModel = JsonSerializer.Serialize(Model);
        await base.OnInitializedAsync();
    }
    private Task<bool?> ConfirmDiscardAsync() => DialogService.ShowMessageBoxAsync("تغییرات ذخیره نشده", "تغییرات کنار گذاشته شود؟", yesText: "کنار گذاشتن", cancelText: "ادامه ویرایش");
    private async Task CancelAsync() { if (!IsDirty || await ConfirmDiscardAsync() == true) { _originalModel = JsonSerializer.Serialize(Model); MudDialog.Cancel(); } }
    private async Task ConfirmNavigationAsync(LocationChangingContext context) { if (IsDirty && await ConfirmDiscardAsync() != true) context.PreventNavigation(); }
}
