using Mkx.Templates.Client.Pages.UserAccounts.ViewModels;
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Abstractions;
using Microsoft.AspNetCore.Components;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Client.Pages.UserAccounts.Components.Editors;

public partial class PasswordEditor
{
    [CascadingParameter] private DialogSession Dialog { get; set; } = default!;

    private readonly PasswordEditorVm _model = new();

    private void Close() => Dialog.Cancel();

    private async Task Submit()
    {
        if (IsBusy) return;
        var request = new UpdateUserPasswordRequest(_model.Password!, _model.NewPassword!);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.UpdateUserPasswordAsync(request, ct),
            afterSend: () =>
            {
                Dialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }
}
