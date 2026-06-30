namespace Mkx.Templates.Shared.DTOs.UserAccounts;

public record GetTwoFactorAuthStatusResponse(bool Enabled, bool MachineRemembered, int RecoveryCodesLeft);
