using System;

namespace Mkx.Templates.Shared.DTOs.UserAccounts;

public record GetUserAccountResponse(Guid Id, string Username, string FullName, string? PhoneNumber, string? Email, string Role, bool IsActive);
