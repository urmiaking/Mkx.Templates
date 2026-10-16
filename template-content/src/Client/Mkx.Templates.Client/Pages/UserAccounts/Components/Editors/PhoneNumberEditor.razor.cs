using Mkx.Templates.Shared.DTOs.UserAccounts;
using Mkx.Templates.Shared.Utilities;
using Mkx.Templates.Sdk.Shared.Extensions;
using Mkx.Templates.Shared.Abstractions;
using Microsoft.AspNetCore.Components;
using Mkx.Templates.Client.Components.UI;

namespace Mkx.Templates.Client.Pages.UserAccounts.Components.Editors;

public partial class PhoneNumberEditor
{
    [CascadingParameter] private DialogSession Dialog { get; set; } = default!;
    [Parameter, EditorRequired] public string PhoneNumber { get; set; } = "";

    private string? _oldPhoneNumber;
    private UiForm _form = default!;
    private string? _token;
    private bool _tokenSent;
    private bool _sendTokenDisabled;
    private int _otpRemaining;
    private CancellationTokenSource? _otpCts;
    private Task _otpTimer = Task.CompletedTask;

    private bool PhoneNumberChanged => _oldPhoneNumber != PhoneNumber;

    protected override void OnInitialized()
    {
        _oldPhoneNumber = PhoneNumber;
        base.OnInitialized();
    }

    private void Close() => Dialog.Cancel();

    private async Task Submit()
    {
        if (IsBusy) return;
        _token = OtpCode.Normalize(_token);
        PhoneNumber = PhoneNumber.ToEnglishNumericsChars().Replace(" ", "");
        if (_token.Length != 6 || !PhoneNumberChanged)
            return;

        await _form.ValidateAsync();

        if (IsBusy || !_form.IsValid)
            return;

        var request = new UpdateUserPhoneNumberRequest(PhoneNumber, _token);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.UpdateUserPhoneNumberAsync(request, ct),
            afterSend: () =>
            {
                Dialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private async Task SendToken()
    {
        PhoneNumber = PhoneNumber.ToEnglishNumericsChars().Replace(" ", "");
        if (IsBusy || _sendTokenDisabled || !PhoneNumberChanged || PhoneNumber.Length != 11 || string.IsNullOrEmpty(_oldPhoneNumber))
            return;

        var request = new SendVerificationCodeRequest(_oldPhoneNumber, PhoneNumber);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.SendVerificationTokenAsync(request, ct),
            afterSend: () =>
            {
                _tokenSent = true;
                _sendTokenDisabled = true;
                _token = "";
                _otpTimer = StartOtpTimerAsync();
                AddInfoToast($"کد فعالسازی به شماره {_oldPhoneNumber} ارسال شد");
                return Task.CompletedTask;
            });
    }

    private async Task StartOtpTimerAsync()
    {
        _otpRemaining = 120;

        _otpCts?.Cancel();
        _otpCts?.Dispose();
        _otpCts = new CancellationTokenSource();

        try
        {
            while (_otpRemaining > 0)
            {
                await Task.Delay(1000, _otpCts.Token);
                _otpRemaining--;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException) { return; }

        _sendTokenDisabled = false;
        if (!IsDisposed) await InvokeAsync(StateHasChanged);
    }

    public override async ValueTask DisposeAsync()
    {
        _otpCts?.Cancel();
        await _otpTimer;
        _otpCts?.Dispose();
        await base.DisposeAsync();
    }
}
