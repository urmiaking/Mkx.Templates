using Mkx.Templates.Client.Pages.UserAccounts.Validators;
using Mkx.Templates.Client.Pages.UserAccounts.ViewModels;
using Mkx.Templates.Shared.Abstractions;
using Microsoft.AspNetCore.Components;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Client.Pages.UserAccounts.Components.Editors;

public partial class UserEditor
{
    [Parameter] public UserEditorVm Model { get; set; } = new();
    [CascadingParameter] private DialogSession Dialog { get; set; } = default!;

    private UiForm _form = default!;
    private readonly UserValidator _userValidator = new();

    private void Close() => Dialog.Cancel();

    private async Task Submit()
    {
        if (IsBusy) return;
        await _form.ValidateAsync();

        if (IsBusy || !_form.IsValid)
            return;

        var isEdit = Model.Id.HasValue;

        var request = Model.ToRequest();

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) =>
                isEdit ? s.UpdateAccountAsync(Model.Id!.Value, request, ct) : s.CreateAccountAsync(request, ct),
            afterSend: () =>
            {
                AddSuccessToast(isEdit ? "ویرایش حساب کاربری با موفقیت انجام شد" : "حساب کاربری با موفقیت ایجاد شد");
                Dialog.Close(true);
                return Task.CompletedTask;
            });
    }
}
