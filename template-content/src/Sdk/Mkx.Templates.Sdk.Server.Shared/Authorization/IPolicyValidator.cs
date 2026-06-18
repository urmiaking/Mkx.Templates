namespace Mkx.Templates.Sdk.Server.Shared.Authorization;

public interface IPolicyValidator
{
    Task<Guid?> GetUserIdAsync();
    Task<bool> HasPolicyAsync(string policy);
    Task<bool> IsInRoleAsync(string role);
    Task<bool> IsAuthenticatedAsync();
}

