using System.Collections.Generic;

namespace Mkx.Templates.Shared.DTOs.UserAccounts;

public record EnableTwoFactorAuthResponse(IEnumerable<string> RecoveryCodes);
