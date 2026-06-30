using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Mkx.Templates.Sdk.Server.Domain.Identity;

namespace Mkx.Templates.Application.Extensions;

public static class UserManagerExtensions
{
    public static async Task<IdentityResult> SetFullNameAsync(this UserManager<AppUser> userManager, AppUser user, string fullName)
    {
        user.SetName(fullName);
        return await userManager.UpdateAsync(user);
    }
}
