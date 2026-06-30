namespace Mkx.Templates.Shared.DTOs.UserAccounts;

public record SendVerificationCodeRequest(string OldPhoneNumber, string NewPhoneNumber);
