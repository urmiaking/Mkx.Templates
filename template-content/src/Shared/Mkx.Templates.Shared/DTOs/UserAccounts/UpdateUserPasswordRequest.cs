namespace Mkx.Templates.Shared.DTOs.UserAccounts;

public record UpdateUserPasswordRequest(string OldPassword, string NewPassword);
