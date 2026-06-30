using Mkx.Templates.Client.Pages.UserAccounts.ViewModels;
using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Mkx.Templates.Client.Pages.UserAccounts.Components.Editors;

public partial class PasswordEditor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly PasswordEditorVm _model = new();

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        var request = new UpdateUserPasswordRequest(_model.Password!, _model.NewPassword!);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.UpdateUserPasswordAsync(request, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }
}