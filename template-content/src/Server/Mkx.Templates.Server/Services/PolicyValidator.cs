using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Shared.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Mkx.Templates.Server.Services;

[ScopedService]
internal class PolicyValidator(IHttpContextAccessor httpContextAccessor, IAuthorizationService service)
    : IPolicyValidator
{
    private readonly HttpContext _context = httpContextAccessor.HttpContext ?? throw new Exception("HttpContext is required.");

    public Task<bool> IsInRoleAsync(string role)
    {
        var user = _context.User;
        var result = user.IsInRole(role);

        return Task.FromResult(result);
    }

    public Task<Guid?> GetUserIdAsync()
    {
        var user = _context.User;
        var idClaim = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        return Guid.TryParse(idClaim?.Value, out var id) ? Task.FromResult<Guid?>(id) : Task.FromResult<Guid?>(null);
    }

    public async Task<bool> HasPolicyAsync(string policy)
    {
        try
        {
            var user = _context.User;

            var result = await service.AuthorizeAsync(user, policy);
            return result.Succeeded;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(_context.User.Identity?.IsAuthenticated ?? false);
    }
}
