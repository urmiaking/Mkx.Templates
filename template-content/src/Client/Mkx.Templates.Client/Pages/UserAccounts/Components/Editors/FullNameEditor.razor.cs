using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Abstractions;
using Microsoft.AspNetCore.Components;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Client.Pages.UserAccounts.Components.Editors;

public partial class FullNameEditor
{
    [CascadingParameter] private DialogSession Dialog { get; set; } = default!;
    [Parameter, EditorRequired] public string FullName { get; set; }

    private UiForm _form = default!;

    private void Close() => Dialog.Cancel();

    private async Task Submit()
    {
        if (IsBusy) return;
        await _form.ValidateAsync();

        if (IsBusy || !_form.IsValid)
            return;

        var request = new UpdateUserFullNameRequest(FullName);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.UpdateUserFullNameAsync(request, ct),
            afterSend: () =>
            {
                Dialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }
}
