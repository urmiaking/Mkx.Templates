using System.Security.Claims;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Abstractions;

public interface IClaimProvider<in TUser>
{
    Task<List<Claim>> GetClaimsAsync(TUser user);
}

